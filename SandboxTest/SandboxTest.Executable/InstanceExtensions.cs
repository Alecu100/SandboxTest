using System.Diagnostics;
using SandboxTest.Instance;

namespace SandboxTest.Executable
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="ExecutableRunner"/>  and related functionalities.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="ExecutableRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="executablePath">The full path of the executable to run</param>
        /// <param name="isRunningFunc">A method that gets text from the executable standard output and verifies it to see if the executable has started</param>
        /// <param name="executableArguments">Optional arguments to pass to the executable when starting it</param>
        /// <param name="workDirectory">Optionally sets the working directory when starting the executable</param>
        /// <param name="environmentVariables">Optionally sets environment variables of the executable before running it</param>
        /// <returns></returns>
        public static IInstance UseExecutableRunner(this IInstance applicationInstance, string executablePath, Func<string?, bool> isRunningFunc, 
            string? workDirectory = null, List<string>? executableArguments = null, IDictionary<string, string>? environmentVariables = null)
        {
            applicationInstance.UseRunner(new ExecutableRunner(executablePath, isRunningFunc, workDirectory, executableArguments, environmentVariables));
            return applicationInstance;
        }

        /// <summary>
        /// Configures how to run or build the exeutable, for example to replace references to external dependencies in configurations 
        /// so that it can run isolated, in a sandbox or to use instances
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="configureBuildFunc"></param>
        /// <param name="configureRunFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureExecutableRunner(this IInstance instance,
            Func<Task> configureBuildFunc,
            Func<Task>? configureRunFunc = default)
        {
            var executableApplicationRunner = instance.Runner as ExecutableRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected ExecutableApplicationRunner");
            }

            executableApplicationRunner.OnConfigureBuild(configureBuildFunc);
            executableApplicationRunner.OnConfigureRun(configureRunFunc);
            return instance;
        }

        /// <summary>
        /// Configures a reset function for <see cref="ExecutableRunner"/>.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="resetFunc"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureExecutableRunnerReset(this IInstance instance,
            Func<Process, Task> resetFunc)
        {
            var executableApplicationRunner = instance.Runner as ExecutableRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected ExecutableApplicationRunner");
            }
            executableApplicationRunner.OnConfigureReset(resetFunc);
            return instance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="ExecutableController"/> to the given application instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddExecutableController(this IInstance instance, string? name = default)
        {
            var executableApplicationRunner = instance.Runner as IExecutableRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected IExecutableRunner");
            }

            var executableApplicationController = new ExecutableController(name);
            instance.AddController(executableApplicationController);
            return instance;
        }
    }
}
