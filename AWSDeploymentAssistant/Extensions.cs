// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ZetaLongPaths;

namespace AWSDeploymentAssistant
{
    public static class Extensions
    {
        public static void AddDirectory(this ZipArchive archive, ZlpDirectoryInfo directory, IEnumerable<string> whitelistedExtensions, IEnumerable<string> excludePatterns, bool recursive = true)
        {
            Assert.IsNotNull(archive, "An archive must be provided.");

            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            ZlpFileInfo[] files = directory.GetFiles("*", option);

            foreach (ZlpFileInfo file in files)
            {
                if (whitelistedExtensions.Contains(file.Extension))
                {
                    bool skip = (from p in excludePatterns
                                 where file.FullName.Contains(p)
                                 select p).Count() > 0;

                    if (!skip)
                    {
                        string archiveName = file.FullName.Replace(directory.FullName + "\\", string.Empty);

                        archive.CreateEntryFromFile(file.FullName, archiveName, CompressionLevel.Optimal);
                    }
                }
            }
        }

        public static string[] ToArray(this StringCollection collection)
        {
            Assert.IsNotNull(collection, "An collection must be provided.");

            string[] values = new string[collection.Count];

            for (int i = 0; i < collection.Count; i++)
            {
                string value = collection[i];

                values[i] = value;
            }

            return values;
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");
            foreach (var element in source)
                target.Add(element);
        }
    }
}