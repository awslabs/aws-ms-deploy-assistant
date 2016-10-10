// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using CodeDeployPlugin.Properties;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace CodeDeployPlugin
{
    // https://github.com/awslabs/aws-codedeploy-samples/blob/master/applications/SampleApp_Windows/appspec.yml
    public class AppSpec
    {
        public AppSpec()
        {
            this.Version = "0.0";
            this.Files = new FileSection();
            this.Hooks = new HooksSection();
        }

        [YamlMember(Alias = "version", Order = 1, ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
        public string Version { get; set; }

        [YamlMember(Alias = "os", Order = 2, ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
        public string OS { get; set; }

        [YamlMember(Alias = "files", Order = 3)]
        public FileSection Files { get; set; }

        [YamlMember(Alias = "hooks", Order = 4)]
        public HooksSection Hooks { get; set; }
    }

    public class FileSection : HashSet<FileTask>
    {
        public void Add(string source, string destination)
        {
            this.Add(new FileTask()
            {
                Source = source,
                Destination = destination
            });
        }
    }

    public class FileTask
    {
        public FileTask()
        {
            this.Source = "\\";
        }

        [YamlMember(Alias = "source", Order = 1, ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Source { get; set; }

        [YamlMember(Alias = "destination", Order = 2, ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Destination { get; set; }
    }

    public class HooksSection
    {
        public HooksSection()
        {
            this.ApplicationStop = new HookEventCollection();
            this.BeforeInstall = new HookEventCollection();
            this.ApplicationStart = new HookEventCollection();
        }

        [YamlMember(Order = 1)]
        public HookEventCollection ApplicationStop { get; set; }

        [YamlMember(Order = 2)]
        public HookEventCollection BeforeInstall { get; set; }

        [YamlMember(Order = 3)]
        public HookEventCollection AfterInstall { get; set; }

        [YamlMember(Order = 4)]
        public HookEventCollection ApplicationStart { get; set; }

        [YamlMember(Order = 5)]
        public HookEventCollection ValidateService { get; set; }
    }

    public class HookEventCollection : HashSet<HookEvent>
    {
        public void Add(string location)
        {
            this.Add(new HookEvent()
            {
                Location = location
            });
        }
    }

    public class HookEvent
    {
        public HookEvent()
        {
            this.Timeout = Settings.Default.DefaultEventTimeout;
        }

        [YamlMember(Alias = "location", Order = 1, ScalarStyle = YamlDotNet.Core.ScalarStyle.DoubleQuoted)]
        public string Location { get; set; }

        [YamlMember(Alias = "timeout", Order = 2, ScalarStyle = YamlDotNet.Core.ScalarStyle.Plain)]
        public int Timeout { get; set; }
    }
}