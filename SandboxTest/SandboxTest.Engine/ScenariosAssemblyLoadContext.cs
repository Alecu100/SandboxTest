using System.Reflection;
using System.Runtime.Loader;

namespace SandboxTest.Engine
{
    public class ScenariosAssemblyLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public ScenariosAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        protected override Assembly? Load(AssemblyName name)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var foundExistingAssembly = Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null) 
            {
                return foundExistingAssembly;
            }

            foundExistingAssembly = Default.Assemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
            if (foundExistingAssembly != null)
            {
                return foundExistingAssembly;
            }

            string? assemblyPath = _resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
