using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandboxTest.Sample.Application3
{
    public static class HostingExtensions
    {
        public static IHostBuilder ConfigureHost(this IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IConsoleService, ConsoleService>();
#if NET8_0_OR_GREATER
                services.AddKeyedScoped<IConsoleService, ConsoleService>("KeyedConsoleService");
#endif
                services.AddHostedService<KeyedConsoleBackgroundService>();
                services.AddSingleton<IRandomGuidGenerator, RandomGuidGenerator>();
            });
            return builder;
        }
    }
}
