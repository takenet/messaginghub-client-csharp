using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Takenet.MessagingHub.Client
{
    public static class ReferencesUtil
    {
        private static readonly object _loadAssembliesSyncRoot = new object();
        private static bool _referencedAssembliesLoaded;

        public static readonly Func<AssemblyName, bool> IgnoreSystemAndMicrosoftAssembliesFilter =
            a => !a.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) &&
            !a.FullName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Loads all assemblies and its references in a given path.
        /// </summary>
        /// <param name="path">The path to look for assemblies.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="ignoreExceptionLoadingReferencedAssembly">Ignore exceptions when loading a referenced assembly</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static void LoadAssembliesAndReferences(string path, string searchPattern = "*.dll", Func<AssemblyName, bool> assemblyFilter = null, bool ignoreExceptionLoadingReferencedAssembly = false)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern));

            foreach (var filePath in Directory.GetFiles(path, searchPattern))
            {
                LoadAssemblyAndReferences(AssemblyName.GetAssemblyName(filePath), assemblyFilter, ignoreExceptionLoadingReferencedAssembly);
            }
        }

        /// <summary>
        /// Loads an assembly and its references.
        /// Only references that are used are actually loaded, since the .NET compiler ignores assemblies that are not used in the code.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="ignoreExceptionLoadingReferencedAssembly">Ignore exceptions when loading a referenced assembly</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void LoadAssemblyAndReferences(AssemblyName assemblyName, Func<AssemblyName, bool> assemblyFilter = null, bool ignoreExceptionLoadingReferencedAssembly = false)
        {
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));
            var loadedAssemblieNames =
                new HashSet<string>(
                    AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .Select(a => a.GetName().FullName));

            LoadAssemblyAndReferences(assemblyName, assemblyFilter, loadedAssemblieNames, ignoreExceptionLoadingReferencedAssembly);
        }

        private static void LoadAssemblyAndReferences(AssemblyName assemblyName, Func<AssemblyName, bool> assemblyFilter, ISet<string> loadedAssembliesNames, bool ignoreExceptionLoadingReferencedAssembly)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                throw new TypeLoadException($"Could not load the assembly '{assemblyName.FullName}'", ex);
            }

            loadedAssembliesNames.Add(assemblyName.FullName);

            var referencedAssemblyNames =
                assembly.GetReferencedAssemblies()
                    .Where(
                        a =>
                                (assemblyFilter == null || assemblyFilter(a)) && !loadedAssembliesNames.Contains(a.FullName));

            foreach (var referencedAssemblyName in referencedAssemblyNames)
            {
                try
                {
                    LoadAssemblyAndReferences(referencedAssemblyName, assemblyFilter, loadedAssembliesNames, ignoreExceptionLoadingReferencedAssembly);
                }
                catch (Exception ex)
                {
                    if (ignoreExceptionLoadingReferencedAssembly)
                        Trace.WriteLine($"Could not load the referenced assembly '{referencedAssemblyName.FullName}' of assembly '{assemblyName.FullName}'");
                    else
                        throw new TypeLoadException($"Could not load the referenced assembly '{referencedAssemblyName.FullName}' of assembly '{assemblyName.FullName}'", ex);
                }
            }
        }


        /// <summary>
        /// Gets all loaded types in the current <see cref="AppDomain"/>, except the ones in the <c>System</c> and <c>Microsoft</c> namespaces.
        /// </summary>
        /// <param name="loadReferences">Load all referenced assemblies before retrieving the types.</param>
        /// <param name="ignoreExceptionLoadingReferencedAssembly">Ignore exceptions when loading a referenced assembly</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllLoadedTypes(bool loadReferences = true, bool ignoreExceptionLoadingReferencedAssembly = false)
        {
            if (loadReferences)
            {
                if (!_referencedAssembliesLoaded)
                {
                    lock (_loadAssembliesSyncRoot)
                    {
                        if (!_referencedAssembliesLoaded)
                        {
                            try
                            {
                                LoadAssemblyAndReferences(
                                    Assembly.GetExecutingAssembly().GetName(),
                                    IgnoreSystemAndMicrosoftAssembliesFilter,
                                    ignoreExceptionLoadingReferencedAssembly);
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("LIME - An error occurred while loading the referenced assemblies: {0}", ex);
                            }
                            finally
                            {
                                _referencedAssembliesLoaded = true;
                            }
                        }
                    }
                }
            }

            try
            {
                return AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(a => IgnoreSystemAndMicrosoftAssembliesFilter(a.GetName()))
                    .SelectMany(a => a.GetTypes());
            }
            catch (Exception ex)
            {
                Trace.TraceError("LIME - An error occurred while loading all types: {0}", ex);
                throw;
            }
        }
    }
}
