using SandboxTest.Instance.Hosted;
using System.Runtime.Loader;

namespace SandboxTest.Application
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var hostedInstanceData = HostedInstanceData.ParseFromEnvironmentVariables(Environment.GetEnvironmentVariables());
            var assemblyLoadContext = new ContainerHostedInstanceLoadContext(Path.GetFullPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName));
            var hostedInstanceInitializerAssembly = assemblyLoadContext.LoadFromAssemblyPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName);
            var hostedInstanceInitializerType = hostedInstanceInitializerAssembly.GetTypes().First(type => type.IsAssignableTo(typeof(IHostedInstanceInitializer)));
            var hostedInstanceInitializer = (IHostedInstanceInitializer)Activator.CreateInstance(hostedInstanceInitializerType)!;

            await hostedInstanceInitializer.InitalizeAsync(hostedInstanceData);
            return await hostedInstanceInitializer.WaitToFinishAsync();
        }
    }
}
