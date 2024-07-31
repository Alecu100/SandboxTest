using SandboxTest.Instance.Hosted;

namespace SandboxTest.Container
{
    internal class Program
    {
        static async Task<int> Main()
        {
            var hostedInstanceData = HostedInstanceData.ParseFromEnvironmentVariables(Environment.GetEnvironmentVariables());
            hostedInstanceData.HostedInstanceInitializerAssemblyFullName = 
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{hostedInstanceData.HostedInstanceInitializerAssemblyFullName.Substring(hostedInstanceData.MainPath!.Length).Trim('\\', '/')}";
            hostedInstanceData.MainPath = $"{Environment.CurrentDirectory}";
            await Task.Delay(60000);
            var assemblyLoadContext = new ContainerHostedInstanceLoadContext(Path.GetFullPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName));
            var hostedInstanceInitializerAssembly = assemblyLoadContext.LoadFromAssemblyPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName);
            var hostedInstanceInitializerType = hostedInstanceInitializerAssembly.GetTypes().First(type => type.IsAssignableTo(typeof(IHostedInstanceInitializer)));
            var hostedInstanceInitializer = (IHostedInstanceInitializer)Activator.CreateInstance(hostedInstanceInitializerType)!;

            await hostedInstanceInitializer.InitalizeAsync(hostedInstanceData);
            return await hostedInstanceInitializer.WaitToFinishAsync();
        }
    }
}
