// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using Amazon;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AWSDeploymentAssistant
{
    [Verb("build", HelpText = "Runs a build pipeline request."), Serializable]
    public class BuildRequest : Request, ISerializable
    {
        private Guid _RequestId;
        private string _SourcePath;
        private string _S3BucketName;
        private string _AWSCredentialProfile;
        private string _PackageZipName;

        public BuildRequest()
           : this(RegionEndpoint.USEast1.SystemName)
        {  }

        public BuildRequest(string bucketRegion)
            : base(bucketRegion)
        {
            this._RequestId = Guid.NewGuid();
        }

        public BuildRequest(string sourcePath, string bucketName, string bucketRegion, string awsCredentialProfile, string packageZipName)
            : this(bucketRegion)
        {
            this.SourcePath = sourcePath;
            this.S3BucketName = bucketName;
            this.Region = bucketRegion;
            this.AWSCredentialProfile = awsCredentialProfile;
            this.PackageZipName = packageZipName;
        }

        protected BuildRequest(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Assert.IsNotNull(info);

            this._RequestId = Guid.Parse(info.GetString("RequestId"));
            this._SourcePath = info.GetString("SourcePath");
            this._S3BucketName = info.GetString("S3BucketName");
            base._Region = RegionEndpoint.GetBySystemName(info.GetString("Region"));
            this._AWSCredentialProfile = info.GetString("AWSCredentialProfile");
            this._PackageZipName = info.GetString("PackageZipName");
        }

        public Guid RequestId
        {
            get
            {
                return _RequestId;
            }
        }

        [Option("source", Required = true, HelpText = "The path to the build output source files that will be packaged and uploaded to s3. If using this tool in an MSBuild post build event, you this could be the $(TargetDir) environment variable.")]
        public string SourcePath
        {
            get
            {
                return this._SourcePath;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);

                if (value.StartsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                if (value.EndsWith("\""))
                {
                    value = value.Substring(0, (value.Length - 1));
                }

                this._SourcePath = value;
            }
        }

        [Option("bucket", Required = true, HelpText = "The name of the bucket where the package will be uploaded to.")]
        public string S3BucketName
        {
            get
            {
                return this._S3BucketName;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);

                this._S3BucketName = value;
            }
        }        

        [Option("profile", Required = false, HelpText = "The name of the AWS stored credential profile that will be used to authenticate the S3 upload request. This profile must contain credentials for an IAM user that has S3 PutObject permission. No other permissions are required.")]
        public string AWSCredentialProfile
        {
            get
            {
                return this._AWSCredentialProfile;
            }
            set
            {

                this._AWSCredentialProfile = value;
            }
        }

        [Option("zipname", Required = true, HelpText = "The file name that will be used for the package zip file and S3 object key. If using AWS CodePipeline, this must match the S3 location key name specified when creating the CodePipeline. This value cannot include any S3 key prefix.")]
        public string PackageZipName
        {
            get
            {
                return this._PackageZipName;
            }
            set
            {
                Assert.IsNotNullOrEmptyString(value);
                Assert.EndsWith(value, ".zip");

                this._PackageZipName = value;
            }
        }

        [Option(HelpText = "Use to perform dry run of process without completing actions. This will only impact a request that makes an external change, such as the S3 upload in a build request.")]
        public bool WhatIf
        {
            get;
            set;
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("RequestId", this._RequestId.ToString());
            info.AddValue("SourcePath", this._SourcePath);
            info.AddValue("S3BucketName", this._S3BucketName);
            info.AddValue("AWSCredentialProfile", this._AWSCredentialProfile);
            info.AddValue("PackageZipName", this._PackageZipName);
        }

        [Usage(ApplicationAlias = "AWSDeploymentAssistant.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Normal Request", new BuildRequest("us-west-2") { SourcePath = "$(TargetDir)", S3BucketName = "MyBucket", AWSCredentialProfile = "MyProfile", PackageZipName = "MyCodePackage.zip" });
                yield return new Example("Normal Request With Default Region (us-east-1) Note: Region attribute is optional.", new BuildRequest() { SourcePath = "$(TargetDir)", S3BucketName = "MyBucket", AWSCredentialProfile = "MyProfile", PackageZipName = "MyCodePackage.zip" });
                yield return new Example("What If Request", new BuildRequest("us-east-2") { SourcePath = "$(TargetDir)", S3BucketName = "MyBucket", AWSCredentialProfile = "MyProfile", PackageZipName = "MyCodePackage.zip", WhatIf = true });
            }
        }
    }
}