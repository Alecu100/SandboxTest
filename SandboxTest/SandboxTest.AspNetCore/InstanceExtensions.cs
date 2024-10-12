using Microsoft.AspNetCore.Builder;
using SandboxTest.Instance;

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
        public static IInstance UseWebApplicationRunner(this IInstance instance, Func<Task<WebApplicationBuilder>> webApplicationBuilderFunc)
        {
            instance.UseRunner(new WebApplicationRunner(webApplicationBuilderFunc));
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
            Func<WebApplicationBuilder, Task> configureBuildFunc,
            Func<WebApplication, Task>? configureRunFunc = default)
        {
            var hostBuilderApplicationRunner = instance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected web application runner");
            }

            hostBuilderApplicationRunner.OnConfigureBuild(configureBuildFunc);
            hostBuilderApplicationRunner.OnConfigureRun(configureRunFunc);
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

        /// <summary>
        /// Configures the url to use when starting a <see cref="WebApplication"/> for a <see cref="WebApplicationRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureWebApplicationRunnerUrl(this IInstance applicationInstance, string url)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected web application runner");
            }

            hostBuilderApplicationRunner.OnConfigureUrl(url);
            return applicationInstance;
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
