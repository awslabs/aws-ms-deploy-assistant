// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using System;
using System.IO;
using System.Linq;

namespace AWSDeploymentAssistant
{
    /// <summary>
    /// Contains validation assertions for expected case testing on input across code members both from the end user and developers.
    /// </summary>
    public static class Assert
    {
        public static void IsTrue(bool value, string message = "The value is expected to be true.")
        {
            if (value == false)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsFalse(bool value, string message = "The value is expected to be false.")
        {
            if (value == true)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsNull(object value, string message = "The value is expected to be null.")
        {
            if (value != null)
            {
                throw new ArgumentException(string.Format("{0} The value [{1}] is invalid.", message, value));
            }
        }

        public static void IsNotNull(object value, string message = "The value is expected to not be null.")
        {
            if (value == null)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsNullOrEmptyString(string value, string message = "The value is expected to be a null or empty string.")
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                throw new ArgumentException(string.Format("{0} The value [{1}] is invalid.", message, value));
            }
        }

        public static void IsNotNullOrEmptyString(string value, string message = "The value is expected to not be a null or empty string.")
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                throw new ArgumentException(message);
            }
        }

        public static void IsWhitelistedValue(string value, string[] whitelist, StringComparison comparison = StringComparison.OrdinalIgnoreCase, string message = "Non-whitelisted value encountered.")
        {
            Assert.IsNotNull(whitelist);

            bool found = ((from w in whitelist
                           where string.Equals(w, value, comparison)
                           select w).Count() == 1);

            if (found == false)
            {
                throw new ArgumentException(string.Format("{0} The value [{1}] is not permitted.", message, value));
            }
        }

        public static void FileIsNotLocked(FileInfo file, string message = "The file is expected to be unlocked.")
        {
            Assert.IsNotNull(file, string.Format("{0} A file must be provided.", message));

            bool locked = FileSystemUtil.IsFileLocked(file);

            Assert.IsFalse(locked, string.Format("{0} [{1}] is locked.", message, file.FullName));
        }

        public static void FileExists(FileInfo file, string message = "The file is expected to exist.")
        {
            Assert.IsNotNull(file, string.Format("{0} A file must be provided.", message));

            bool exists = File.Exists(file.FullName);

            Assert.IsTrue(exists, string.Format("{0} [{1}] not found.", message, file.FullName));
        }

        public static void FileDoesNotExist(FileInfo file, string message = "The file is expected to not exist.")
        {
            Assert.IsNotNull(file, string.Format("{0} A file must be provided.", message));

            bool exists = File.Exists(file.FullName);

            Assert.IsFalse(exists, string.Format("{0} [{1}] was found.", message, file.FullName));
        }

        public static void DirectoryExists(DirectoryInfo directory, string message = "The directory is expected to exist.")
        {
            Assert.IsNotNull(directory, string.Format("{0} A directory must be provided.", message));
            Assert.DirectoryExists(directory.FullName, message);
        }

        public static void DirectoryDoesNotExist(DirectoryInfo directory, string message = "The directory is expected to not exist.")
        {
            Assert.IsNotNull(directory, string.Format("{0} A directory must be provided.", message));
            Assert.DirectoryDoesNotExist(directory.FullName, message);
        }

        public static void DirectoryExists(string path, string message = "The directory path is expected to exist.")
        {
            Assert.IsNotNullOrEmptyString(path, string.Format("{0} A directory path must be provided.", message));

            bool exists = Directory.Exists(path);

            Assert.IsTrue(exists, string.Format("{0} [{1}] not found.", message, path));
        }

        public static void DirectoryDoesNotExist(string path, string message = "The directory path is expected to not exist.")
        {
            Assert.IsNotNullOrEmptyString(path, string.Format("{0} A directory path must be provided.", message));

            bool exists = Directory.Exists(path);

            Assert.IsFalse(exists, string.Format("{0} [{1}] was found.", message, path));
        }

        public static void StartsWith(string value, string prefix, StringComparison comparision = StringComparison.OrdinalIgnoreCase, string message = "The value does not start with the expected characters.")
        {
            Assert.IsNotNull(value);
            Assert.IsNotNullOrEmptyString(prefix);

            if (value.StartsWith(prefix, comparision) == false)
            {
                throw new ArgumentException(string.Format("{0} [{1}] should start with [{2}]", message, value, prefix));
            }
        }

        public static void EndsWith(string value, string suffix, StringComparison comparision = StringComparison.OrdinalIgnoreCase, string message = "The value does not end with the expected characters.")
        {
            Assert.IsNotNull(value);
            Assert.IsNotNullOrEmptyString(suffix);

            if (value.EndsWith(suffix, comparision) == false)
            {
                throw new ArgumentException(string.Format("{0} [{1}] should end with [{2}]", message, value, suffix));
            }
        }

        public static void StringDoesNotEqual(string value, string invalid, StringComparison comparison = StringComparison.OrdinalIgnoreCase, string message = "The value is not equal to the expected value.")
        {
            if (string.Equals(value, invalid, comparison))
            {
                throw new ArgumentException(string.Format("{0} [{1}] should not equal [{2}]", message, value, invalid));
            }
        }
    }
}