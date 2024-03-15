using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;

namespace SandboxTest.Runners.Host
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="HostApplicationRunner"/>  and related functionalities.
    /// </summary>
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="HostApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="hostBuilderFunc"></param>
        /// <returns></returns>
        public static IApplicationInstance UseHostApplicationRunner(this IApplicationInstance applicationInstance, Func<string[], Task<IHostBuilder>> hostBuilderFunc)
        {
            applicationInstance.UseRunner(new HostApplicationRunner(hostBuilderFunc));
            return applicationInstance;
        }

        /// <summary>
        /// Configures the host in such a way to be able to run in an isolated sandbox independent of external dependencies.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <param name="configureRunSandboxFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureHostApplicationRunnerSandbox(this IApplicationInstance applicationInstance,
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

        /// <summary>
        /// Adds an application controller of type <see cref="HostApplicationController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddHostApplicationController(this IApplicationInstance applicationInstance, string? name = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }

            var hostApplicationController = new HostApplicationController(name);
            applicationInstance.AddController(hostApplicationController);
            return applicationInstance;
        }

        public static IApplicationInstance ConfigureHostApplicationRunnerReset(this IApplicationInstance applicationInstance, 
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
