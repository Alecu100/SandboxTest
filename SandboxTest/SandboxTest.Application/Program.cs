using SandboxTest.Instance.Hosted;
using SandboxTest.Loader;

namespace SandboxTest.Application
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var hostedInstanceData = HostedInstanceData.ParseFromCommandLineArguments(args);
            var scenarioAssemblyLoadContext = new ScenariosAssemblyLoadContext(Path.GetFullPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName));
            IHostedInstanceInitializer? hostedInstanceInitializer = null;

            using (var reflectionContext = scenarioAssemblyLoadContext.EnterContextualReflection())
            {
                var hostedInstanceInitializerAssembly = scenarioAssemblyLoadContext.LoadFromAssemblyPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName);
                var hostedInstanceInitializerType = hostedInstanceInitializerAssembly.GetTypes().First(type => type.IsAssignableTo(typeof(IHostedInstanceInitializer)));
                hostedInstanceInitializer = (IHostedInstanceInitializer)Activator.CreateInstance(hostedInstanceInitializerType)!;
            }

            await hostedInstanceInitializer.InitalizeAsync(scenarioAssemblyLoadContext, hostedInstanceData);
            return await hostedInstanceInitializer.WaitToFinishAsync();
        }
    }
}
