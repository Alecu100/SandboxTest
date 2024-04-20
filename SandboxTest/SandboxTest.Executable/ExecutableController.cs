using System.Diagnostics;

namespace SandboxTest.Executable
{
    public class ExecutableController : IController
    {
        protected readonly string? _name;
        protected Process? _executableProcess;

        /// <summary>
        /// Creates a new instance of the <see cref="WireMockApplicationController"/>.
        /// </summary>
        /// <param name="name"></param>
        public ExecutableController(string? name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the WireMockApplicationController.
        /// </summary>
        public string? Name { get => _name; }

        /// <summary>
        /// Gets the running executable process
        /// </summary>
        public Process ExecutableProcess { get => _executableProcess ?? throw new InvalidOperationException("Executable is not running and executable runner is not built"); }

        /// <summary>
        /// Gets the standard output of the executable to see what it writes to it.
        /// </summary>
        public StreamReader ExecutableOutput { get => _executableProcess?.StandardOutput ?? throw new InvalidOperationException("Executable is not running and executable runner is not built"); }

        /// <summary>
        /// Gets the standard error of the executable to see what it errors it writes to it.
        /// </summary>
        public StreamReader ExecutableError { get => _executableProcess?.StandardError ?? throw new InvalidOperationException("Executable is not running and executable runner is not built"); }

        /// <summary>
        /// Gets the standard input of the executable to be able to sends commands to it if needed.
        /// </summary>
        public StreamWriter ExecutableInput { get => _executableProcess?.StandardInput ?? throw new InvalidOperationException("Executable is not running and executable runner is not built"); }

        /// <summary>
        /// Fetches the WireMock server instance in order to expose it to be configured to return specific responses for http requests.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task BuildAsync(IInstance applicationInstance)
        {
            var executableApplicationRunner = applicationInstance.Runner as IExecutableRunner;
            if (executableApplicationRunner == null)
            {
                throw new InvalidOperationException("Executable application controller can only be used with a executable application runnner");
            }
            if (executableApplicationRunner.ExecutableProcess == null)
            {
                throw new InvalidOperationException($"Executable application is not built and running");
            }
            _executableProcess = executableApplicationRunner.ExecutableProcess;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Not used for executable application controller
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public virtual Task ConfigureBuildAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Not used for executable application controller
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public virtual Task ResetAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }
    }
}
