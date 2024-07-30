using System.Reflection;
using System.Runtime.Loader;

namespace SandboxTest.Container
{
    public class ContainerHostedInstanceLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public ContainerHostedInstanceLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        protected override Assembly? Load(AssemblyName name)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var foundExistingAssembly = currentAssemblies.FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
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
