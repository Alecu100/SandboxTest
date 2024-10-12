using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using SandboxTest.Utils;

namespace SandboxTest.Engine
{
    /// <summary>
    /// Represents the standard assembly loader used in SandBoxTest in a completely separate assembly from the rest of the assemblies
    /// in order to avoid assembly loading issues such as asssembly duplication.
    /// </summary>
    public class ScenariosAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly List<AssemblyDependencyResolver> _frameworkResolvers;
        private readonly List<string> _forceLoadAssemblieNames;

        public ScenariosAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
        {
            _forceLoadAssemblieNames = new List<string>();
            _frameworkResolvers = new List<AssemblyDependencyResolver>();
            var runtimeDirectory = PathUtils.LocateFolderPath(RuntimeEnvironment.GetRuntimeDirectory(), "dotnet")!;
            var sharedDirectory = PathUtils.AppendToPath(runtimeDirectory, "shared");
            var frameworkDirectories = Directory.GetDirectories(sharedDirectory) ?? Enumerable.Empty<string>();
            var runtimeVersion = Environment.Version;
            foreach (var frameworkDirectory in frameworkDirectories)
            {
                string? matchedFrameworkVersionDirectory = null;
                Version? matchedFrameworkVersion = null;
                var frameworkVersionDirectories = Directory.GetDirectories(frameworkDirectory) ?? Enumerable.Empty<string>();
                foreach (var frameworkVersionDirectory in frameworkVersionDirectories)
                {
                    var test = PathUtils.GetDirectoryNameOnly(frameworkVersionDirectory);
                    if (Version.TryParse(PathUtils.GetDirectoryNameOnly(frameworkVersionDirectory), out Version? version))
                    {
                        if (runtimeVersion.Major == version.Major && (matchedFrameworkVersion == null || version.MinorRevision > matchedFrameworkVersion.MinorRevision))
                        {
                            matchedFrameworkVersion = version;
                            matchedFrameworkVersionDirectory = frameworkVersionDirectory;
                        }
                    }
                }

                if (matchedFrameworkVersionDirectory != null)
                {
                    var frameworkAssemblies = Directory.GetFiles(matchedFrameworkVersionDirectory);
                    if (frameworkAssemblies != null && frameworkAssemblies.Any())
                        _frameworkResolvers.Add(new AssemblyDependencyResolver(frameworkAssemblies.First()));
                }
            }
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        /// <summary>
        /// Forces the loading of the assemblies with the specified names in the current assembly load context;
        /// </summary>
        /// <param name="assemblieNames">The assembly names to force load.</param>
        public void ForceLoadAssemblies(params string[] assemblieNames)
        {
            _forceLoadAssemblieNames.AddRange(assemblieNames);
        }

        public void ClearForceLoadedAssemblies()
        {
            _forceLoadAssemblieNames.Clear();
        }

        protected override Assembly? Load(AssemblyName name)
        {
            var scenarioAssemblyLoadContextAssembly = GetType().Assembly;
            if (AssemblyName.ReferenceMatchesDefinition(scenarioAssemblyLoadContextAssembly.GetName(), name))
            {
                return scenarioAssemblyLoadContextAssembly;
            }

            var foundExistingAssembly = Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null)
            {
                return foundExistingAssembly;
            }

            foundExistingAssembly = Default.Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null && !IsForceLoadedAssembly(name))
            {
                return foundExistingAssembly;
            }
            foundExistingAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null && !IsForceLoadedAssembly(name))
            {
                return foundExistingAssembly;
            }

            var assemblyPath = _resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            if (assemblyPath == null)
            {
                foreach (var frameworkResolver in _frameworkResolvers)
                {
                    assemblyPath = frameworkResolver.ResolveAssemblyToPath(name);
                    if (assemblyPath != null)
                    {
                        return LoadFromAssemblyPath(assemblyPath);
                    }
                }
            }

            return null;
        }

        private void InitializeRuntimeAssemblyResolvers(string currentDirectory)
        {
            _frameworkResolvers.Add(new AssemblyDependencyResolver(currentDirectory));
            var childDirectories = Directory.GetDirectories(currentDirectory);

            if (childDirectories != null && childDirectories.Any())
            {
                foreach (var childDirectoy in childDirectories)
                {
                    InitializeRuntimeAssemblyResolvers(childDirectoy);
                }
            }
        }

        private bool IsForceLoadedAssembly(AssemblyName assemblyName)
        {
            return _forceLoadAssemblieNames.Any(forceLoadedAssemblyName => assemblyName.Name!.Equals(forceLoadedAssemblyName));
        }
    }
}
