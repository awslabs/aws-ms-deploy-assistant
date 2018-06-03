// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.

using Amazon;
using CommandLine;
using System.Runtime.Serialization;

namespace AWSDeploymentAssistant
{
    public class Request : ISerializable
    {
        protected RegionEndpoint _Region;

        internal Request()
        { }

        protected Request(string region)
            : this()
        {
            this.Region = region;
        }

        protected Request(SerializationInfo info, StreamingContext context)
        {
            Assert.IsNotNull(info);

            this._Region = RegionEndpoint.GetBySystemName(info.GetString("Region"));
        }

        [Option("region", Required = false, HelpText = "The AWS region.")]

        public string Region
        {
            get {
                if (this._Region == null) {
                    Amazon.Runtime.AppConfigAWSRegion appConfig = new Amazon.Runtime.AppConfigAWSRegion();
                    this._Region = appConfig.Region;

                    if (this._Region == null) {
                        this._Region = RegionEndpoint.USEast1;
                    }
                }
                return this._Region.SystemName;
            }
            set {
                if (!string.IsNullOrEmpty(value)) {
                    this._Region = RegionEndpoint.GetBySystemName(value);
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Region", this._Region.SystemName);
        }
    }
}