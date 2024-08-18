using SandboxTest.Instance.Hosted;
using SandboxTest.Loader;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class HostedInstanceInitializer : IHostedInstanceInitializer
    {
        private ScenariosAssemblyLoadContext? _scenariosAssemblyLoadContext;
        private object? _hostedInstanceLoop;
        private string? _mainPath;
        private string? _assemblySourceName;

        public HostedInstanceInitializer()
        {
        }

        ///<inheritdoc/>
        public async Task InitalizeAsync(HostedInstanceData hostedInstanceData)
        {
            _mainPath = hostedInstanceData.MainPath;
            _assemblySourceName = hostedInstanceData.AssemblySourceName;
            _scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext($"{_mainPath}{Path.DirectorySeparatorChar}{_assemblySourceName}");
            _scenariosAssemblyLoadContext.ForceLoadAssemblies(typeof(IHostedInstanceInitializer).Assembly.GetName().Name!, typeof(HostedInstanceInitializer).Assembly.GetName().Name!);

            using (var contextualReflectionScope = _scenariosAssemblyLoadContext.EnterContextualReflection())
            {
                _scenariosAssemblyLoadContext.LoadFromAssemblyName(typeof(Scenario).Assembly.GetName());
                var hostedInstanceLoopAssembly = _scenariosAssemblyLoadContext.LoadFromAssemblyName(typeof(HostedInstanceInitializer).Assembly.GetName());
                _scenariosAssemblyLoadContext.ClearForceLoadedAssemblies();
                var hostedInstanceLoopType = hostedInstanceLoopAssembly.GetType(typeof(HostedInstanceLoop).FullName!)!;
                _hostedInstanceLoop = Activator.CreateInstance(hostedInstanceLoopType)!;
                var hostedInstanceLoopInitializeMethod = _hostedInstanceLoop.GetType().GetMethod(nameof(HostedInstanceLoop.StartAsync), BindingFlags.Public | BindingFlags.Instance);
                var startLoopTask = (Task)hostedInstanceLoopInitializeMethod!.Invoke(_hostedInstanceLoop, new object[] {_scenariosAssemblyLoadContext,  hostedInstanceData.ToDictionary() })!;
                await startLoopTask;
            }
        }

        ///<inheritdoc/>
        public async Task<int> WaitToFinishAsync()
        {
            if (_hostedInstanceLoop == null || _scenariosAssemblyLoadContext == null)
            {
                throw new InvalidOperationException("Host initializer not initialized");
            }

            using (var contextualReflectionScope = _scenariosAssemblyLoadContext!.EnterContextualReflection())
            {
                var hostedInstanceLoopInitializeMethod = _hostedInstanceLoop.GetType().GetMethod(nameof(HostedInstanceLoop.WaitToStopAsync), BindingFlags.Public | BindingFlags.Instance);
                var stopLoopTask = (Task<int>)hostedInstanceLoopInitializeMethod!.Invoke(_hostedInstanceLoop, null)!;
                return await stopLoopTask;
            }
        }
    }
}
