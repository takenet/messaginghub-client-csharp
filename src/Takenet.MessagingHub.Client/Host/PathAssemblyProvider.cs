using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Gets all assemblies in a given path.
    /// </summary>
    public sealed class PathAssemblyProvider : IAssemblyProvider
    {
        private readonly string _path;

        public PathAssemblyProvider(string path)
        {
            _path = path;
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            foreach (var assemblyPath in Directory.GetFiles(_path, "*.dll"))
            {
                Assembly assembly = null;

                try
                {
                    assembly = LoadAssembly(Path.GetFullPath(assemblyPath));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }

                if (assembly != null) yield return assembly;
            }
        }

        private static Assembly LoadAssembly(string assemblyPath)
        {
            return Assembly.LoadFrom(assemblyPath);
        }
    }
}