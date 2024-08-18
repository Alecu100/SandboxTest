using SandboxTest.Utils;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace SandboxTest.Engine
{
    public class ScenariosAssemblyLoadContext : AssemblyLoadContext
    {
        private const string SandboxTestAssemblyName = nameof(SandboxTest);
        private const string SandboxTestEngineAssemblyName = $"{nameof(SandboxTest)}.{nameof(Engine)}";

        private AssemblyDependencyResolver _resolver;
        private List<AssemblyDependencyResolver> _frameworkResolvers;

        public ScenariosAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
        {
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
        /// Gets or sets whether to force loading of all assemblies in the current scenario assembly load context.
        /// </summary>
        public bool ForceLoadingSandboxTestAssembliesInCurrentContext { get; set; }


        protected override Assembly? Load(AssemblyName name)
        {
            var foundExistingAssembly = Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null)
            {
                return foundExistingAssembly;
            }

            foundExistingAssembly = Default.Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null && (!ForceLoadingSandboxTestAssembliesInCurrentContext || !IsSandboxTestAssembly(name)))
            {
                return foundExistingAssembly;
            }
            foundExistingAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null && (!ForceLoadingSandboxTestAssembliesInCurrentContext || !IsSandboxTestAssembly(name)))
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

        private bool IsSandboxTestAssembly(AssemblyName assemblyName)
        {
            return assemblyName.Name!.Equals(SandboxTestAssemblyName) || assemblyName.Name.Equals(SandboxTestEngineAssemblyName);
        }
    }
}
