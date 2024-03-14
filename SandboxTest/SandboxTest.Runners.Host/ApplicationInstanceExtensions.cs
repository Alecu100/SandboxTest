using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;

namespace SandboxTest.Runners.Host
{
    public static class ApplicationInstanceExtensions
    {
        public static IApplicationInstance UseHostApplicationRunner(this IApplicationInstance applicationInstance, Func<string[], Task<IHostBuilder>> hostBuilderFunc)
        {
            applicationInstance.UseRunner(new HostApplicationRunner(hostBuilderFunc));
            return applicationInstance;
        }

        public static IApplicationInstance ConfigureHostBuilderApplicationSandbox(this IApplicationInstance applicationInstance,
            Func<IHostBuilder, Task> configureBuildSandboxFunc,
            Func<IHost, Task>? configureRunSandboxFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }

            hostBuilderApplicationRunner.OnConfigureBuildSandbox(configureBuildSandboxFunc);
            hostBuilderApplicationRunner.OnConfigureRunSandbox(configureRunSandboxFunc);
            return applicationInstance;
        }

        public static IApplicationInstance AssignHostApplicationController(this IApplicationInstance applicationInstance, string? name)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }

            var hostApplicationController = new HostApplicationController(name);
            applicationInstance.AssignController(hostApplicationController);
            return applicationInstance;
        }

        public static IApplicationInstance ConfigureReset(this IApplicationInstance applicationInstance, 
            Func<IHost, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
