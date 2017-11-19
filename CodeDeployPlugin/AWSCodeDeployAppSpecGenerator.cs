// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using AWSDeploymentAssistant;
using CodeDeployPlugin.Properties;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ZetaLongPaths;

namespace CodeDeployPlugin
{
    public class AWSCodeDeployAppSpecGenerator : IDeploymentTask
    {
        public static string[] OSOptions = new string[2] { "linux", "windows" };

        private Dictionary<string, string> m_Options;

        public AWSCodeDeployAppSpecGenerator()
        {
            this.m_Options = new Dictionary<string, string>();
        }

        public int Priority
        {
            get
            {
                return Settings.Default.Priority;
            }
        }

        public bool ThrowOnError
        {
            get
            {
                return Settings.Default.ThrowOnError;
            }
        }

        public string Name
        {
            get
            {
                return "AWSCodeDeployPlugin";
            }
        }

        public bool LoadOptions
        {
            get
            {
                return true;
            }
        }

        public Dictionary<string, string> Options
        {
            get
            {
                return this.m_Options;
            }
        }

        public void Execute(BuildRequest request, ZlpDirectoryInfo workingDirectory)
        {
            // https://docs.aws.amazon.com/codedeploy/latest/userguide/writing-app-spec.html
            try
            {
                ZlpFileInfo appSpecFile = (from f in workingDirectory.GetFiles("appspec.yml", SearchOption.TopDirectoryOnly)
                                        select f).SingleOrDefault();

                if (appSpecFile == null)
                {
                    Program.Logger.Warn("Generating AppSpec file. For advanced configuration create and include an appspec.yml at the root of your project. See: https://docs.aws.amazon.com/codedeploy/latest/userguide/app-spec-ref-structure.html");

                    string appSpecFilePath = Path.Combine(workingDirectory.FullName, "appspec.yml");

                    appSpecFile = new ZlpFileInfo(appSpecFilePath);

                    AppSpec appSpec = this.BuildAppSpec(request, workingDirectory);

                    using (FileStream stream = appSpecFile.OpenCreate())
                    {
                        using (StreamWriter sWriter = new StreamWriter(stream))
                        {
                            using (StringWriter writer = new StringWriter())
                            {
                                YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();
                                serializer.Serialize(writer, appSpec);
                                var yaml = writer.ToString();
                                sWriter.WriteLine(yaml);
                                Program.Logger.Info("----------------------- BEGIN APPSPEC -----------------------");
                                Program.Logger.Info(yaml);
                                Program.Logger.Info("----------------------- END APPSPEC -----------------------");
                            }
                        }
                    }
                }
                else
                {
                    Program.Logger.Info("    An AppSpec file was found in the working directory. This plug-in will not generate an AppSec.");
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex);
                throw ex;
            }
        }

        internal AppSpec BuildAppSpec(BuildRequest request, ZlpDirectoryInfo workingDirectory)
        {
            var os = (from d in this.Options
                      where string.Equals(d.Key, "os", System.StringComparison.Ordinal)
                      select d.Value).SingleOrDefault() ?? "windows";

            Assert.IsWhitelistedValue(os, AWSCodeDeployAppSpecGenerator.OSOptions);

            var destination = (from d in this.Options
                               where string.Equals(d.Key, "destination", System.StringComparison.Ordinal)
                               select d.Value).SingleOrDefault() ?? "c:\\inetpub\\wwwroot";

            var applicationStop = (from d in this.Options
                                   where string.Equals(d.Key, "applicationstop", System.StringComparison.Ordinal)
                                   select d.Value).SingleOrDefault();

            var beforeInstall = (from d in this.Options
                                 where string.Equals(d.Key, "beforeinstall", System.StringComparison.Ordinal)
                                 select d.Value).SingleOrDefault();

            var afterInstall = (from d in this.Options
                                where string.Equals(d.Key, "afterinstall", System.StringComparison.Ordinal)
                                select d.Value).SingleOrDefault();

            var applicationStart = (from d in this.Options
                                    where string.Equals(d.Key, "applicationstart", System.StringComparison.Ordinal)
                                    select d.Value).SingleOrDefault();

            var validateService = (from d in this.Options
                                   where string.Equals(d.Key, "validateservice", System.StringComparison.Ordinal)
                                   select d.Value).SingleOrDefault();

            if (string.IsNullOrEmpty(applicationStop))
            {
                applicationStop = @"defaultApplicationStop.ps1";
                string path = Path.Combine(workingDirectory.FullName, applicationStop);
                FileSystemUtil.WriteFile(path, Resources.defaultApplicationStop);
            }
            if (string.IsNullOrEmpty(beforeInstall))
            {
                beforeInstall = @"defaultBeforeInstall.ps1";
                string path = Path.Combine(workingDirectory.FullName, beforeInstall);
                string commandPath = Path.Combine(destination, "*");
                string command = "Remove-Item \"" + commandPath + "\"  -Force -Recurse";
                FileSystemUtil.WriteFile(path, Resources.defaultBeforeInstall, command);
            }
            if (string.IsNullOrEmpty(applicationStart))
            {
                applicationStart = @"defaultApplicationStart.ps1";
                string path = Path.Combine(workingDirectory.FullName, applicationStart);
                FileSystemUtil.WriteFile(path, Resources.defaultApplicationStart);
            }

            AppSpec appSpec = new AppSpec()
            {
                OS = os
            };
            appSpec.Files.Add("\\", destination);
            appSpec.Hooks.ApplicationStop.Add(applicationStop);
            appSpec.Hooks.BeforeInstall.Add(beforeInstall);
            appSpec.Hooks.ApplicationStart.Add(applicationStart);

            if (afterInstall != null)
            {
                appSpec.Hooks.AfterInstall = new HookEventCollection();
                appSpec.Hooks.AfterInstall.Add(afterInstall);
            }
            if (validateService != null)
            {
                appSpec.Hooks.ValidateService = new HookEventCollection();
                appSpec.Hooks.ValidateService.Add(validateService);
            }

            return appSpec;
        }

        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Dispose();
            }
        }

        public void Dispose()
        { }
    }
}