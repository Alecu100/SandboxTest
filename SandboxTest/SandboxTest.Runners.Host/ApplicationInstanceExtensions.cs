using Microsoft.Extensions.Hosting;

namespace SandboxTest.Runners.Host
{
    public static class ApplicationInstanceExtensions
    {
        public static ApplicationInstance UseHostBuilderApplicationRunner(this ApplicationInstance applicationInstance, Func<string[], Task<IHostBuilder>> hostBuilderFunc)
        {
            applicationInstance.AssingRunner(new HostBuilderApplicationRunner(hostBuilderFunc));
            return applicationInstance;
        }

        public static ApplicationInstance ConfigureHostBuilderApplicationSandbox(this ApplicationInstance applicationInstance,
            Func<IHostBuilder, Task> configureBuildSandboxFunc,
            Func<IHost, Task>? configureRunSandboxFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostBuilderApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }

            hostBuilderApplicationRunner.OnConfigureBuildSandbox(configureBuildSandboxFunc);
            hostBuilderApplicationRunner.OnConfigureRunSandbox(configureRunSandboxFunc);
            return applicationInstance;
        }

        public static ApplicationInstance ConfigureReset(this ApplicationInstance applicationInstance, 
            Func<IHost, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostBuilderApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
