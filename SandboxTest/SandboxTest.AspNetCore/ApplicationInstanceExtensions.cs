using Microsoft.AspNetCore.Builder;

namespace SandboxTest.AspNetCore
{
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="WebApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="webApplicationBuilderFunc"></param>
        /// <returns></returns>
        public static IApplicationInstance UseWebApplicationRunner(this IApplicationInstance applicationInstance, Func<string[], Task<WebApplicationBuilder>> webApplicationBuilderFunc)
        {
            applicationInstance.UseRunner(new WebApplicationRunner(webApplicationBuilderFunc));
            return applicationInstance;
        }

        /// <summary>
        /// Configures the web application in such a way to be able to run in an isolated sandbox independent of external dependencies.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <param name="configureRunSandboxFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureWebApplicationRunnerSandbox(this IApplicationInstance applicationInstance,
            Func<WebApplicationBuilder, Task> configureBuildSandboxFunc,
            Func<WebApplication, Task>? configureRunSandboxFunc = default)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WebApplicationRunner");
            }

            hostBuilderApplicationRunner.OnConfigureBuildSandbox(configureBuildSandboxFunc);
            hostBuilderApplicationRunner.OnConfigureRunSandbox(configureRunSandboxFunc);
            return applicationInstance;
        }

        /// <summary>
        /// Configures the arguments to use when creating the <see cref="WebApplicationBuilder"/> for a <see cref="WebApplicationRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureWebApplicationRunnerArguments(this IApplicationInstance applicationInstance, params string[] arguments)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WebApplicationRunner");
            }

            hostBuilderApplicationRunner.OnConfigureArguments(arguments);
            return applicationInstance;
        }


        /// <summary>
        /// Configures the url to use when starting a <see cref="WebApplication"/> for a <see cref="WebApplicationRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureWebApplicationRunnerUrl(this IApplicationInstance applicationInstance, string url)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WebApplicationRunner");
            }

            hostBuilderApplicationRunner.OnConfigureUrl(url);
            return applicationInstance;
        }

        public static IApplicationInstance ConfigureHostApplicationRunnerReset(this IApplicationInstance applicationInstance,
        Func<WebApplication, Task>? resetFunc)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as WebApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WebApplicationRunner");
            }
            hostBuilderApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }
    }
}
