using System.Reflection;
using System.Reflection.Emit;

namespace SandboxTest.Hosting.ServiceInterceptor.Internal
{
    /// <summary>
    /// Creates a service interceptor assembly for an assembly that contains services which need to be intercepted.
    /// </summary>
    public class ServiceInterceptorAssemblyBuilder
    {
        private readonly Assembly _interceptedTypesAssembly;

        public ServiceInterceptorAssemblyBuilder(Assembly interceptedTypesAssembly)
        {
            _interceptedTypesAssembly = interceptedTypesAssembly;
        }

        /// <summary>
        /// Builds the assembly along with the module.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ServiceInterceptorAssembly Build()
        {
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{_interceptedTypesAssembly.GetName().Name}.{Guid.NewGuid()}.dll");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));

            return new ServiceInterceptorAssembly(assemblyBuilder, moduleBuilder);
        }
    }
}
