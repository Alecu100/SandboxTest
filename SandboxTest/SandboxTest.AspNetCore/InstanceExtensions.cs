using Microsoft.AspNetCore.Builder;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.AspNetCore
{
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="WebApplicationRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="webApplicationBuilderFunc"></param>
        /// <returns></returns>
        public static IInstance UseWebApplicationRunner(this IInstance instance, Func<Task<WebApplicationBuilder>> webApplicationBuilderFunc, string url)
        {
            instance.UseRunner(new WebApplicationRunner(webApplicationBuilderFunc, url));
            return instance;
        }

        /// <summary>
        /// Configures the web application in such a way to be able to run in an scenario.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="configureBuildFunc"></param>
        /// <param name="configureRunFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureWebApplicationRunner(this IInstance instance,
            Func<WebApplicationBuilder, Task>? configureBuildFunc,
            Func<WebApplication, Task>? configureRunFunc = default)
        {
            var webApplicationRunner = instance.Runner as WebApplicationRunner;
            if (webApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected web application runner");
            }

            if (configureBuildFunc != null)
            {
                Func<IScenarioSuiteContext, Task> onConfigureBuild = async (ctx) =>
                {
                    await configureBuildFunc(webApplicationRunner.WebApplicationBuilder);
                };
                webApplicationRunner.AddAttachedMethod(AttachedMethodType.RunnerToRunner, onConfigureBuild, nameof(configureBuildFunc), nameof(WebApplicationRunner.ConfigureBuildAsync), 100);
            }
            if (configureRunFunc != null)
            {
                Func<IScenarioSuiteContext, Task> onConfigureRun = async (ctx) =>
                {
                    await configureRunFunc(webApplicationRunner.WebApplication);
                };
                webApplicationRunner.AddAttachedMethod(AttachedMethodType.RunnerToRunner, onConfigureRun, nameof(configureRunFunc), nameof(WebApplicationRunner.RunAsync), -100);
            }

            return instance;
        }

        /// <summary>
        /// Adds an controller of type <see cref="WebApplicationController"/> to the given instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddWebApplicationController(this IInstance instance, string? name = default)
        {
            var webApplicationRunner = instance.Runner as IWebApplicationRunner;
            if (webApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected web application runner");
            }

            var hostApplicationController = new WebApplicationController(name);
            instance.AddController(hostApplicationController);
            return instance;
        }

        public static IInstance ConfigureWebApplicationRunnerReset(this IInstance applicationInstance,
        Func<WebApplication, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected web application runner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
