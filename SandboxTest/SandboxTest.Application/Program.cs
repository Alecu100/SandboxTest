using System.Runtime.Loader;

namespace SandboxTest.Engine.ApplicationContainer
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var hostedInstanceData = HostedInstanceData.ParseFromCommandLineArguments(args);
            var hostedInstanceInitializerAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName);
            var hostedInstanceInitializerType = hostedInstanceInitializerAssembly.GetTypes().First(type => type.IsAssignableTo(typeof(IHostedInstanceInitializer)));
            var hostedInstanceInitializer = (IHostedInstanceInitializer)Activator.CreateInstance(hostedInstanceInitializerType)!;

            await hostedInstanceInitializer.InitalizeAsync(hostedInstanceData);
            return await hostedInstanceInitializer.WaitToFinishAsync();
        }
    }
}
