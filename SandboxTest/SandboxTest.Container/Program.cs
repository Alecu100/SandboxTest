using SandboxTest.Instance.Hosted;

namespace SandboxTest.Container
{
    internal class Program
    {
        static async Task<int> Main()
        {
            var hostedInstanceData = HostedInstanceData.ParseFromEnvironmentVariables(Environment.GetEnvironmentVariables());
            Console.WriteLine(string.Join(';', hostedInstanceData.ToCommandLineArguments()));
            Console.WriteLine($"Environment.CurrentDirectory {Environment.CurrentDirectory} is null {Environment.CurrentDirectory}");
            Console.WriteLine($"Path.DirectorySeparatorChar {Path.DirectorySeparatorChar} is null {Path.DirectorySeparatorChar}");
            Console.WriteLine($"hostedInstanceData.HostedInstanceInitializerTypeFullName {hostedInstanceData.HostedInstanceInitializerTypeFullName} is null {hostedInstanceData.HostedInstanceInitializerTypeFullName}");
            Console.WriteLine($"hostedInstanceData.MainPath {hostedInstanceData.MainPath} is null {hostedInstanceData.MainPath == null}");
            hostedInstanceData.HostedInstanceInitializerAssemblyFullName = 
                $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{hostedInstanceData.HostedInstanceInitializerAssemblyFullName.Substring(hostedInstanceData.MainPath.Length).Trim('\\', '/')}";
            hostedInstanceData.MainPath = $"{Environment.CurrentDirectory}";
            Console.WriteLine("Host data updated");
            Console.WriteLine(string.Join(';', hostedInstanceData.ToCommandLineArguments()));
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
