using Microsoft.AspNetCore.Builder;

namespace SandboxTest.AspNetCore
{
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="WebRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="webApplicationBuilderFunc"></param>
        /// <returns></returns>
        public static IInstance UseWebRunner(this IInstance applicationInstance, Func<string[], Task<WebApplicationBuilder>> webApplicationBuilderFunc)
        {
            applicationInstance.UseRunner(new WebRunner(webApplicationBuilderFunc));
            return applicationInstance;
        }

        /// <summary>
        /// Configures the web application in such a way to be able to run in an isolated sandbox independent of external dependencies.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="configureBuildFunc"></param>
        /// <param name="configureRunFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureWebRunner(this IInstance applicationInstance,
            Func<WebApplicationBuilder, Task> configureBuildFunc,
            Func<WebApplication, Task>? configureRunFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected WebRunner");
            }

            hostBuilderApplicationRunner.OnConfigureBuild(configureBuildFunc);
            hostBuilderApplicationRunner.OnConfigureRun(configureRunFunc);
            return applicationInstance;
        }

        /// <summary>
        /// Configures the arguments to use when creating the <see cref="WebApplicationBuilder"/> for a <see cref="WebRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureWebRunnerArguments(this IInstance applicationInstance, params string[] arguments)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected WebRunner");
            }

            hostBuilderApplicationRunner.OnConfigureArguments(arguments);
            return applicationInstance;
        }


        /// <summary>
        /// Configures the url to use when starting a <see cref="WebApplication"/> for a <see cref="WebRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureWebRunnerUrl(this IInstance applicationInstance, string url)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected WebRunner");
            }

            hostBuilderApplicationRunner.OnConfigureUrl(url);
            return applicationInstance;
        }

        public static IInstance ConfigureWebRunnerReset(this IInstance applicationInstance,
        Func<WebApplication, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected WebRunner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
