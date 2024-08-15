using Newtonsoft.Json.Serialization;
using SandboxTest.Loader;
using System.Runtime.Caching;
using System.Runtime.Loader;

namespace SandboxTest.Engine.Utils
{
    public class ScenarioAssemblyLoadContextSerializationBinder : ISerializationBinder
    {
        private ScenariosAssemblyLoadContext _scenariosAssemblyLoadContext;
        private MemoryCache _typeNameCache;

        public ScenarioAssemblyLoadContextSerializationBinder(ScenariosAssemblyLoadContext scenariosAssemblyLoadContext)
        {
            _scenariosAssemblyLoadContext = scenariosAssemblyLoadContext;
            _typeNameCache = new MemoryCache(Guid.NewGuid().ToString());
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = serializedType.Assembly.GetName().Name;
            typeName = serializedType.FullName == null ? serializedType.Name : serializedType.FullName;
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            var typeNameKey = GetTypeNameCacheKey(assemblyName, typeName);
            var type = _typeNameCache.Get(typeNameKey) as Type;
            if (type != null)
            {
                return type;
            }

            if (assemblyName == null)
            {
                type = _scenariosAssemblyLoadContext.Assemblies.Select(assembly => assembly.GetType(typeName)).FirstOrDefault(type => type != null);
                if (type != null)
                {
                    _typeNameCache.Add(typeNameKey, type, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
                    return type;
                }

                type = AssemblyLoadContext.Default.Assemblies.Select(assembly => assembly.GetType(typeName)).FirstOrDefault(type => type != null);
                if (type != null)
                {
                    _typeNameCache.Add(typeNameKey, type, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
                    return type;
                }

                throw new InvalidOperationException($"Could not find type for typeName {typeName}");
            }

            var typeAssembly = _scenariosAssemblyLoadContext.Assemblies.FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
            if (typeAssembly == null) 
            {
                typeAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
            }
            if (typeAssembly == null)
            {
                throw new InvalidOperationException($"Could not find assembly for assembly name {assemblyName}");
            }

            type = typeAssembly.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Could not find type for typeName {typeName}");
            }
            _typeNameCache.Add(typeNameKey, type, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMinutes(5) });
            return type;
        }

        private string GetTypeNameCacheKey(string? assemblyName, string typeName)
        {
            if (assemblyName == null)
            {
                return typeName;
            }

            return $"{assemblyName}-{typeName}";
        }
    }
}
