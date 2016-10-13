// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AWSDeploymentAssistant.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;

namespace AWSDeploymentAssistant
{
    internal class Pipeline
    {
        private BuildRequest _Request;

        internal Pipeline(BuildRequest request)
        {
            this.Request = request;
        }

        internal BuildRequest Request
        {
            get
            {
                return this._Request;
            }
            private set
            {
                Assert.IsNotNull(value);

                Assert.StringDoesNotEqual(value.AWSCredentialProfile, Program.DefaultArgumentValue, StringComparison.OrdinalIgnoreCase, "Invalid credential profile value.");
                Assert.StringDoesNotEqual(value.PackageZipName, Program.DefaultArgumentValue, StringComparison.OrdinalIgnoreCase, "Invalid package zip file name value.");
                Assert.StringDoesNotEqual(value.S3BucketName, Program.DefaultArgumentValue, StringComparison.OrdinalIgnoreCase, "Invalid S3 bucket name value.");
                Assert.StringDoesNotEqual(value.S3BucketRegion, Program.DefaultArgumentValue, StringComparison.OrdinalIgnoreCase, "Invalid S3 bucket region value.");
                Assert.StringDoesNotEqual(value.SourcePath, Program.DefaultArgumentValue, StringComparison.OrdinalIgnoreCase, "Invalid request source directory path value.");
                this._Request = value;
            }
        }

        internal DirectoryInfo TempContentDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(this.TempDirectory.FullName, "content"));
            }
        }

        internal DirectoryInfo TempDirectory { get; private set; }

        internal FileInfo OutputFile { get; private set; }

        internal bool Run()
        {
            bool result = false;

            Program.Logger.Info("Starting deployment pipeline.");

            try
            {
                Program.Logger.Info("Creating working directory for request.");
                this.PopulateRequestWorkingDirectory();
                Program.Logger.Info("    Completed creating working directory for request.");

                Program.Logger.Info("Running plugins.");
                this.RunPlugins();
                Program.Logger.Info("    Completed running plugins.");

                Program.Logger.Info("Creating package zip.");
                this.ZipPackage();
                Program.Logger.Info("    Completed creating package zip.");

                Program.Logger.Info("Uploading package to S3.");
                this.UploadPackageToS3();
                Program.Logger.Info("    Completed uploading package to S3.");

                result = true;
            }
            finally
            {
                Program.Logger.Info("Serializing request object, temp directory object, temp content directory object and output file object.");
                Program.Logger.Info(JsonConvert.SerializeObject(this.Request));
                Program.Logger.Info(JsonConvert.SerializeObject(this.TempDirectory));
                Program.Logger.Info(JsonConvert.SerializeObject(this.TempContentDirectory));
                Program.Logger.Info(JsonConvert.SerializeObject(this.OutputFile));

                if (this.Request.WhatIf)
                {
                    Program.Logger.Info("The current request is a whatif request. The request working directory was not cleaned to enable review.");
                }
                else
                {
                    try
                    {
                        if ((this.TempDirectory != null) && Directory.Exists(this.TempDirectory.FullName))
                        {
                            this.TempDirectory.Delete(true);
                            Program.Logger.Info("Cleaning-up request working directory.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error("Failed to delete temporary directory.", ex);
                    }
                }

                Program.Logger.Info("Finished deployment pipeline.");
            }

            return result;
        }

        private void PopulateRequestWorkingDirectory()
        {
            Assert.DirectoryExists(this.Request.SourcePath, "Unable to find request source directory path.");

            string tempPath = Path.Combine(Program.TempFolderPath, this.Request.RequestId.ToString());

            this.TempDirectory = new DirectoryInfo(tempPath);

            Assert.DirectoryDoesNotExist(this.TempDirectory, "The request working directory already exists.");

            this.TempDirectory.Create();

            Assert.DirectoryExists(this.TempDirectory, "Failed to create request working directory.");

            this.TempContentDirectory.Create();

            DirectoryInfo sourceDirectory = new DirectoryInfo(this.Request.SourcePath);

            FileSystemUtil.CopyDirectory(sourceDirectory, this.TempContentDirectory, true);
        }

        private void RunPlugins()
        {
            DirectoryInfo taskDirectory = new DirectoryInfo(Program.TaskPluginFolderPath);

            var assemblyFiles = taskDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

            Program.Logger.InfoFormat("Found [{0}] plugin assemblies.", assemblyFiles.Count());

            foreach (var assemblyFile in assemblyFiles)
            {
                var assembly = Assembly.LoadFile(assemblyFile.FullName);

                var pluginTypes = (from type in assembly.GetTypes()
                                   where typeof(IDeploymentTask).IsAssignableFrom(type)
                                   select type);

                Program.Logger.InfoFormat("Found [{0}] plugin tasks in [{1}] assembly.", pluginTypes.Count(), assembly.FullName);

                List<IDeploymentTask> plugins = new List<IDeploymentTask>();

                foreach (var pluginType in pluginTypes)
                {
                    using (var plugin = (IDeploymentTask)(Activator.CreateInstance(pluginType.Assembly.FullName, pluginType.FullName).Unwrap()))
                    {
                        plugins.Add(plugin);
                    }
                }

                foreach (var plugin in plugins.OrderBy(p => p.Priority))
                {
                    try
                    {
                        Program.Logger.InfoFormat("Executing [{0}] plugin.", plugin.GetType().FullName);
                        plugin.Execute(this.Request, this.TempContentDirectory);
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Failed to execute [{0}] plugin.", plugin.GetType().FullName);

                        if (plugin.ThrowOnError)
                        {
                            throw new InvalidOperationException(message, ex);
                        }
                        else
                        {
                            Program.Logger.Error(message, ex);
                        }
                    }
                    finally
                    {
                        Program.Logger.InfoFormat("    Completed executing [{0}] plugin.", plugin.GetType().FullName);
                    }

                    plugin.Dispose();
                }
            }
        }

        private void ZipPackage()
        {
            Assert.DirectoryExists(this.TempContentDirectory, "Unable to find request wokring directory.");

            string zipPath = Path.Combine(this.TempDirectory.FullName, this.Request.PackageZipName);

            this.OutputFile = new FileInfo(zipPath);

            Assert.FileDoesNotExist(this.OutputFile, "The package zip file already exists.");

            using (var fileStream = new FileStream(this.OutputFile.FullName, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    var include = Settings.Default.PublishFileTypesIncludePattern.ToArray();
                    var exclude = Settings.Default.PublishFileTypesExcludePattern.ToArray();

                    archive.AddDirectory(this.TempContentDirectory, include, exclude);
                }
            }
        }

        private void UploadPackageToS3()
        {
            Assert.FileExists(this.OutputFile, "The package zip file has not been created.");
            Assert.FileIsNotLocked(this.OutputFile, "The package zip file must be unlocked to allow hash generation.");

            MD5 hasher = MD5.Create();

            byte[] hash = null;

            using (FileStream stream = this.OutputFile.Open(FileMode.Open, FileAccess.Read))
            {
                hash = hasher.ComputeHash(stream);
            }

            Assert.FileIsNotLocked(this.OutputFile, "The package zip file must be unlocked so that they S3 SDK can access and upload.");

            AWSCredentials credentials = Program.GetAWSCredentials(this.Request.AWSCredentialProfile);

            RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(this.Request.S3BucketRegion);

            using (AmazonS3Client client = new AmazonS3Client(credentials, endpoint))
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = this.Request.S3BucketName,
                    FilePath = this.OutputFile.FullName,
                    CannedACL = S3CannedACL.BucketOwnerFullControl,
                    Key = this.Request.PackageZipName,
                    MD5Digest = Convert.ToBase64String(hash)
                };

                if (this.Request.WhatIf == false)
                {
                    PutObjectResponse putResponse = client.PutObject(request);

                    if (putResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new HttpException(int.Parse(putResponse.HttpStatusCode.ToString()), "Failed to upload package to S3.");
                    }
                }
            }
        }
    }
}