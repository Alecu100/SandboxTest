using SandboxTest.Runners.Executables;
using System.Diagnostics;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="ExecutableApplicationRunner"/>  and related functionalities.
    /// </summary>
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="ExecutableApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="executablePath">The full path of the executable to run</param>
        /// <param name="isRunningFunc">A method that gets text from the executable standard output and verifies it to see if the executable has started</param>
        /// <param name="executableArguments">Optional arguments to pass to the executable when starting it</param>
        /// <param name="workDirectory">Optionally sets the working directory when starting the executable</param>
        /// <param name="environmentVariables">Optionally sets environment variables of the executable before running it</param>
        /// <returns></returns>
        public static IApplicationInstance UseExecutableApplicationRunner(this IApplicationInstance applicationInstance, string executablePath, Func<string?, bool> isRunningFunc, 
            string? workDirectory = null, List<string>? executableArguments = null, IDictionary<string, string>? environmentVariables = null)
        {
            applicationInstance.UseRunner(new ExecutableApplicationRunner(executablePath, isRunningFunc, workDirectory, executableArguments, environmentVariables));
            return applicationInstance;
        }

        /// <summary>
        /// Configures the executable in such a way to be able to run in an isolated sandbox independent of external dependencies by changing the configuration files or even source files.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <param name="configureRunSandboxFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureExecutableApplicationRunnerSandbox(this IApplicationInstance applicationInstance,
            Func<Task> configureBuildSandboxFunc,
            Func<Task>? configureRunSandboxFunc = default)
        {
            var executableApplicationRunner = applicationInstance.Runner as ExecutableApplicationRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected ExecutableApplicationRunner");
            }

            executableApplicationRunner.OnConfigureBuildSandbox(configureBuildSandboxFunc);
            executableApplicationRunner.OnConfigureRunSandbox(configureRunSandboxFunc);
            return applicationInstance;
        }

        /// <summary>
        /// Configures a reset function for <see cref="ExecutableApplicationRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="resetFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance ConfigureExecutableApplicationRunnerReset(this IApplicationInstance applicationInstance,
            Func<Process, Task> resetFunc)
        {
            var executableApplicationRunner = applicationInstance.Runner as ExecutableApplicationRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected ExecutableApplicationRunner");
            }
            executableApplicationRunner.OnConfigureReset(resetFunc);
            return applicationInstance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="ExecutableApplicationController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddExecutableApplicationControllerr(this IApplicationInstance applicationInstance, string? name = default)
        {
            var executableApplicationRunner = applicationInstance.Runner as IExecutableApplicationRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WireMockApplicationRunner");
            }

            var executableApplicationController = new ExecutableApplicationController(name);
            applicationInstance.AddController(executableApplicationController);
            return applicationInstance;
        }
    }
}
