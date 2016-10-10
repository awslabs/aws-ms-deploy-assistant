// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using Amazon.Runtime;
using Amazon.Util;
using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace AWSDeploymentAssistant
{
    [Verb("profile", HelpText = "Runs a stored credential profile request.")]
    internal class ProfileRequest : Request
    {
        private string _ProfileName;
        private string _AccessKey;
        private string _SecretKey;

        public ProfileRequest()
        { }

        [Option('a', "action", Default = ProfileRequestAction.None, Required = true, HelpText = "A profile request action. Options: (List, GetProfile, IsKnown, Register, Unregister) - values are case sensitive.")]
        public ProfileRequestAction Action { get; set; }

        [Option('n', "name", Default = Program.DefaultArgumentValue, HelpText = "Required for GetProfile, IsKonwn, RegisterProfile and UnregisterProfile Actions. A profile name.")]
        public string ProfileName
        {
            get
            {
                return this._ProfileName;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);

                this._ProfileName = value;
            }
        }

        [Option("accesskey", Default = Program.DefaultArgumentValue, HelpText = "Required Only for RegisterProfile Action. An AWS IAM user access key.")]
        public string AccessKey
        {
            get
            {
                return this._AccessKey;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);

                this._AccessKey = value;
            }
        }

        [Option("secretkey", Default = Program.DefaultArgumentValue, HelpText = "Required Only for RegisterProfile Action. An AWS IAM user secret key.")]
        public string SecretKey
        {
            get
            {
                return this._SecretKey;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);

                this._SecretKey = value;
            }
        }

        public ProfileSettingsBase Profile
        {
            get
            {
                if (!this.Known)
                {
                    if (!string.IsNullOrEmpty(this.AccessKey) && !string.IsNullOrEmpty(this.SecretKey))
                    {
                        Program.RegisterAWSCredentialProfile(this.ProfileName, this.AccessKey, this.SecretKey);
                    }
                }

                return Program.GetAWSCredentialProfile(this.ProfileName);
            }
        }

        public AWSCredentials Credential
        {
            get
            {
                return Program.GetAWSCredentials(this.ProfileName);
            }
        }

        public bool Known
        {
            get
            {
                return Program.IsAWSCredentialProfileKnown(this.ProfileName);
            }
        }

        public IEnumerable<ProfileSettingsBase> AllLocalProfiles
        {
            get
            {
                return Program.ListAWSCredentialProfiles();
            }
        }

        public bool Unregistered
        {
            get
            {
                Program.UnregisterAWSCredentialProfile(this.ProfileName);

                return this.Known;
            }
        }

        [Usage(ApplicationAlias = "AWSDeploymentAssistant.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("List Profiles", new ProfileRequest { Action = ProfileRequestAction.List });
                yield return new Example("Get Profile", new ProfileRequest { Action = ProfileRequestAction.GetProfile, ProfileName = "MyProfile" });
                yield return new Example("Check If Profile Is Known", new ProfileRequest { Action = ProfileRequestAction.IsKnown, ProfileName = "MyProfile" });
                yield return new Example("Register Profile", new ProfileRequest { Action = ProfileRequestAction.Register, ProfileName = "MyProfile", AccessKey = "<KEY VALUE>", SecretKey = "<KEY VALUE>" });
                yield return new Example("Unregister Profile", new ProfileRequest { Action = ProfileRequestAction.Unregister, ProfileName = "MyProfile" });
            }
        }
    }
}