using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandboxTest.Sample.Application1
{
    public static class HostingExtensions
    {
        public static IHostBuilder ConfigureHost(this IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IConsoleService, ConsoleService>();
                services.AddKeyedScoped<IConsoleService, ConsoleService>("KeyedConsoleService");
                services.AddHostedService<ConsoleBackgroundService>();
                services.AddSingleton<IRandomGuidGenerator, RandomGuidGenerator>();
            });
            return builder;
        }
    }
}
