using SandboxTest.Instance.Hosted;
using SandboxTest.Loader;

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
            IHostedInstanceInitializer? hostedInstanceInitializer = null;

            var scenarioAssemblyLoadContext = new ScenariosAssemblyLoadContext(Path.GetFullPath(hostedInstanceData.HostedInstanceInitializerAssemblyFullName));
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
