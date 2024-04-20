using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="HostRunner"/>  and related functionalities.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="HostRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="hostBuilderFunc"></param>
        /// <returns></returns>
        public static IInstance UseHostRunner(this IInstance applicationInstance, Func<string[], Task<IHostBuilder>> hostBuilderFunc)
        {
            applicationInstance.UseRunner(new HostRunner(hostBuilderFunc));
            return applicationInstance;
        }

        /// <summary>
        /// Configures the host to be able to run it in tests such as changing the application configuration Urls to
        /// point to other instances from a scenario.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="configureBuildFunc"></param>
        /// <param name="configureRunFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureHostRunner(this IInstance applicationInstance,
            Func<IHostBuilder, Task> configureBuildFunc,
            Func<IHost, Task>? configureRunFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected HostRunner");
            }

            hostBuilderApplicationRunner.OnConfigureBuild(configureBuildFunc);
            hostBuilderApplicationRunner.OnConfigureRun(configureRunFunc);
            return applicationInstance;
        }

        /// <summary>
        /// Configures the arguments to use when creating the <see cref="HostBuilder"/> for a <see cref="HostRunner"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureHostRunnerArguments(this IInstance instance, params string[] arguments)
        {
            var hostBuilderApplicationRunner = instance.Runner as HostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected HostRunner");
            }

            hostBuilderApplicationRunner.OnConfigureArguments(arguments);
            return instance;
        }

        /// <summary>
        /// Adds an controller of type <see cref="HostController"/> to the given instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddHostController(this IInstance applicationInstance, string? name = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as IHostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on application instance, expected HostRunner");
            }

            var hostApplicationController = new HostController(name);
            applicationInstance.AddController(hostApplicationController);
            return applicationInstance;
        }

        public static IInstance ConfigureHostApplicationRunnerReset(this IInstance applicationInstance,
            Func<IHost, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected HostRunner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
