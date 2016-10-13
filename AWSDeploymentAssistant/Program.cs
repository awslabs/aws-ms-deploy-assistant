// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using Amazon.Runtime;
using Amazon.Util;
using AWSDeploymentAssistant.Properties;
using CommandLine;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AWSDeploymentAssistant
{
    public class Program
    {
        public const string LogFolderPath = "C:\\temp\\AWS Development Tools\\EC2 Deployment Assistant\\logs";
        public const string TempFolderPath = "C:\\temp\\AWS Development Tools\\EC2 Deployment Assistant\\temp";

        internal const string DefaultArgumentValue = "foo-bar";

        private const int SUCCESS = 0;
        private const int ERROR_BAD_ARGUMENTS = 1;
        private const int ERROR_BUILD_REQUEST_FAILED = 2;
        private const int ERROR_PROFILE_REQUEST_FAILED = 3;

        static Program()
        {
            log4net.GlobalContext.Properties["LogPath"] = Program.LogFolderPath;
            Program.Logger = LogManager.GetLogger(Settings.Default.DefaultLoggerName);

            {
                var path = Path.Combine(Program.AssemblyPath, "plugins");
                Assert.DirectoryExists(path, "Unable to find task plugin folder path.");
                Program.TaskPluginFolderPath = path;
            }
        }

        public static void ConfigureSettings(ParserSettings settings)
        {
            settings.EnableDashDash = true;
            settings.CaseSensitive = false;
            settings.IgnoreUnknownArguments = false;
            settings.HelpWriter = Console.Out;
        }

        private static void Main(string[] args)
        {
            int exitCode = Program.ERROR_BAD_ARGUMENTS;

            XmlConfigurator.Configure();

            try
            {
                if (Directory.Exists(Program.TempFolderPath) == false)
                {
                    Directory.CreateDirectory(Program.TempFolderPath);
                }

                if (Directory.Exists(Program.LogFolderPath) == false)
                {
                    Directory.CreateDirectory(Program.LogFolderPath);
                }

                using (CommandLine.Parser parser = new Parser(settings => Program.ConfigureSettings(settings)))
                {
                    var result = parser.ParseArguments<ProfileRequest, BuildRequest>(args)
                        .WithParsed<ProfileRequest>(request => exitCode = RunProfileRequest(request))
                        .WithParsed<BuildRequest>(request => exitCode = RunBuildReqeust(request))
                        .WithNotParsed(errors =>
                        {
                            foreach (Error error in errors)
                            {
                                Type t = error.GetType();

                                if (t.IsAssignableFrom(typeof(NoVerbSelectedError)))
                                {
                                    Program.Logger.Fatal(error.Tag);
                                    exitCode = Program.ERROR_BAD_ARGUMENTS;
                                }
                                else if (t.IsAssignableFrom(typeof(BadFormatTokenError)) ||
                                         t.IsAssignableFrom(typeof(BadVerbSelectedError)) ||
                                         t.IsAssignableFrom(typeof(UnknownOptionError)))
                                {
                                    Program.Logger.Error(error.Tag);
                                    exitCode = Program.ERROR_BAD_ARGUMENTS;
                                }
                                else if (t.IsAssignableFrom(typeof(HelpRequestedError)) |
                                         t.IsAssignableFrom(typeof(HelpVerbRequestedError)))
                                {
                                    // Do Nothing
                                    exitCode = Program.SUCCESS;
                                }
                                else if (t.IsAssignableFrom(typeof(MissingRequiredOptionError)) ||
                                         t.IsAssignableFrom(typeof(MissingValueOptionError)) ||
                                         t.IsAssignableFrom(typeof(MutuallyExclusiveSetError)) ||
                                         t.IsAssignableFrom(typeof(RepeatedOptionError)) ||
                                         t.IsAssignableFrom(typeof(SequenceOutOfRangeError)))
                                {
                                    Program.Logger.Error(error.Tag);
                                    exitCode = Program.ERROR_BAD_ARGUMENTS;
                                }
                                else if (t.IsAssignableFrom(typeof(VersionRequestedError)))
                                {
                                    // Do Nothing
                                    exitCode = Program.SUCCESS;
                                }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Fatal("An error occurred while parsing command line arguments.", ex);
                Environment.Exit(Program.ERROR_BAD_ARGUMENTS);
            }
            finally
            {
                Environment.Exit(exitCode);
            }
        }

        private static int RunProfileRequest(ProfileRequest request)
        {
            int result = Program.ERROR_PROFILE_REQUEST_FAILED;

            try
            {
                switch (request.Action)
                {
                    case ProfileRequestAction.GetCredential:
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request.Credential));
                            break;
                        }
                    case ProfileRequestAction.GetProfile:
                    case ProfileRequestAction.Register:
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request.Profile));
                            break;
                        }
                    case ProfileRequestAction.IsKnown:
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request.Known));
                            break;
                        }
                    case ProfileRequestAction.List:
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request.AllLocalProfiles));
                            break;
                        }
                    case ProfileRequestAction.Unregister:
                        {
                            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(request.Unregistered));
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException("The specified profile request action type is not supported.");
                        }
                }

                result = Program.SUCCESS;
                Console.WriteLine();
                Console.WriteLine("Press any key to continue.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Program.Logger.Fatal("An error occurred while running profile request.", ex);
            }

            return result;
        }

        private static int RunBuildReqeust(BuildRequest request)
        {
            int result = Program.ERROR_BUILD_REQUEST_FAILED;

            try
            {
                Pipeline pipeline = new Pipeline(request);

                if (pipeline.Run())
                {
                    result = Program.SUCCESS;
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Fatal("An error occurred while running pipeline.", ex);
            }

            return result;
        }

        public static ILog Logger
        {
            get;
            private set;
        }

        public static string TaskPluginFolderPath
        {
            get;
            private set;
        }

        internal static string AssemblyPath
        {
            get
            {
                Assembly a = Assembly.GetExecutingAssembly();

                string fullPath = a.Location;

                string directoryPath = Path.GetDirectoryName(fullPath);

                return directoryPath;
            }
        }

        internal static void RegisterAWSCredentialProfile(string profileName, string accessKey, string secretKey)
        {
            Amazon.Util.ProfileManager.RegisterProfile(profileName, accessKey, secretKey);
        }

        internal static void UnregisterAWSCredentialProfile(string profileName)
        {
            Amazon.Util.ProfileManager.UnregisterProfile(profileName);
        }

        internal static IEnumerable<ProfileSettingsBase> ListAWSCredentialProfiles()
        {
            return Amazon.Util.ProfileManager.ListProfiles();
        }

        public static bool IsAWSCredentialProfileKnown(string profileName)
        {
            return Amazon.Util.ProfileManager.IsProfileKnown(profileName);
        }

        internal static ProfileSettingsBase GetAWSCredentialProfile(string profileName)
        {
            return Amazon.Util.ProfileManager.GetProfile(profileName);
        }

        public static AWSCredentials GetAWSCredentials(string profileName)
        {
            return Amazon.Util.ProfileManager.GetAWSCredentials(profileName);
        }
    }
}