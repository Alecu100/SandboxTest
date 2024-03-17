using Microsoft.Extensions.Hosting;

namespace SandboxTest.Sample.Application3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.ConfigureHost();
            await hostBuilder.RunConsoleAsync();
        }
    }
}
