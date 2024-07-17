using System.Reflection.Emit;


namespace SandboxTest.Hosting.ServiceInterceptor.Internal
{
    public class ServiceInterceptorAssembly
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;

        public ServiceInterceptorAssembly(AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder)
        {
            _assemblyBuilder = assemblyBuilder;
            _moduleBuilder = moduleBuilder;
        }

        public AssemblyBuilder AssemblyBuilder => _assemblyBuilder;

        public ModuleBuilder ModuleBuilder => _moduleBuilder;
    }
}
