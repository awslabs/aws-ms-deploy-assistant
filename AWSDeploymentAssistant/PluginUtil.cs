// Copyright 2016-2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// Licensed under the Apache License, Version 2.0  (the "License"). You may not use this file except in compliance
// with the License. A copy of the License is located at http://aws.amazon.com/apache2.0/ or in the "license" file
// accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the specific language governing permissions and limitations
// under the License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AWSDeploymentAssistant
{
    internal class PluginUtil
    {
        internal static void RunPlugins(FileInfo assemblyFile, BuildRequest request, DirectoryInfo tempDirectory)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var domain = AppDomain.CreateDomain(assemblyFile.Name);

            domain.ResourceResolve += Domain_ResourceResolve;

            var proxy = (PluginProxyDomain)domain.CreateInstanceAndUnwrap(executingAssembly.FullName, typeof(PluginProxyDomain).FullName);

            proxy.ExecutePlugins(assemblyFile, request, tempDirectory);
        }

        private static Assembly Domain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            var resources = executingAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));

            if (resources.Count() > 0)
            {
                var resourceName = resources.First();
                using (Stream stream = executingAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    var block = new byte[stream.Length];
                    stream.Read(block, 0, block.Length);
                    return Assembly.Load(block);
                }
            }
            else
            {
                var files = Directory.GetFiles(Program.TaskPluginFolderPath);

                var domain = (PluginProxyDomain)sender;

                foreach (var file in files)
                {
                    Assembly testAssembly = Assembly.LoadFrom(file);

                    var testAssemblyResources = testAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));

                    if (testAssemblyResources.Count() > 0)
                    {
                        var resourceName = testAssemblyResources.First();
                        using (Stream stream = testAssembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream == null) return null;
                            var block = new byte[stream.Length];
                            stream.Read(block, 0, block.Length);
                            return Assembly.Load(block);
                        }
                    }
                }
            }
            return null;
        }

        private class PluginProxyDomain : MarshalByRefObject
        {
            public void ExecutePlugins(FileInfo assemblyFile, BuildRequest request, DirectoryInfo tempDirectory)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(assemblyFile.FullName);

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
                            plugin.Execute(request, tempDirectory);
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
                catch (Exception ex)
                {
                    Program.Logger.Error(ex);
                    throw ex;
                }
            }
        }
    }
}