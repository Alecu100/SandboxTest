using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

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
        public static IInstance UseHostRunner(this IInstance applicationInstance, Func<Task<IHostBuilder>> hostBuilderFunc)
        {
            applicationInstance.UseRunner(new HostRunner(hostBuilderFunc));
            return applicationInstance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="ServiceInterceptorController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddServiceIncerpeptorController(this IInstance applicationInstance)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as IHostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }
            if (applicationInstance.Controllers.Any(controller => controller is ServiceInterceptorController))
            {
                throw new InvalidOperationException("Application instance already has one service interceptor controller assigned, only one can be assigned per application instance");
            }

            var hostApplicationController = new ServiceInterceptorController();
            applicationInstance.AddController(hostApplicationController);
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
            Func<IHostBuilder, Task>? configureBuildFunc,
            Func<IHost, Task>? configureRunFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as HostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected HostRunner");
            }

            if (configureBuildFunc != null)
            {
                Func<IScenarioSuiteContext, Task> onConfigureBuild = async (ctx) =>
                {
                    await configureBuildFunc(hostBuilderApplicationRunner.HostBuilder);
                };
                hostBuilderApplicationRunner.AddAttachedMethod(AttachedMethodType.RunnerToRunner, onConfigureBuild, nameof(configureBuildFunc), nameof(HostRunner.ConfigureBuildAsync), 100);
            }
            if (configureRunFunc != null)
            {
                Func<IScenarioSuiteContext, Task> onConfigureRun = async (ctx) =>
                {
                    await configureRunFunc(hostBuilderApplicationRunner.Host);
                };
                hostBuilderApplicationRunner.AddAttachedMethod(AttachedMethodType.RunnerToRunner, onConfigureRun, nameof(configureRunFunc), nameof(HostRunner.RunAsync), -100);
            }

            return applicationInstance;
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
