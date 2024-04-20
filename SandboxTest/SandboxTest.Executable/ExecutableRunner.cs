using System.Diagnostics;

namespace SandboxTest.Executable
{
    /// <summary>
    /// Represents an runner that actually starts another executable as a child process, capturing it's outputs and inputs.
    /// </summary>
    public class ExecutableRunner : IExecutableRunner
    {
        protected Process? _buildProcess;
        protected TaskCompletionSource<bool>? _isDoneBuildingTaskCompletionSource;
        protected string? _executableBuildPath;
        protected List<string>? _buildArguments;
        protected string? _buildWorkDirectory;
        protected IDictionary<string, string>? _buildEnvironmentVariables;
        protected Func<Task>? _configureBuildSandboxFunc;

        protected Process? _executableProcess;
        protected Func<string?, bool> _isRunningFunc;
        protected string _executablePath;
        protected string? _runWorkDirectory;
        protected IDictionary<string, string>? _executableEnvironmentVariables;
        protected Func<Task>? _configureRunSandboxFunc;
        protected TaskCompletionSource<bool>? _isRunningTaskCompletionSource;
        protected List<string>? _executableArguments;
        protected Func<Process, Task>? _resetFunc;

        ///<inheritdoc/>
        public Process ExecutableProcess => _executableProcess ?? throw new InvalidOperationException("Executable not started");

        ///<inheritdoc/>
        public string ExecutablePath => _executablePath ?? throw new InvalidOperationException("Executable path not set");

        /// <summary>
        /// Creates a new instance of <see cref="ExecutableRunner"/>
        /// </summary>
        /// <param name="executablePath">The full path of the executable to run </param>
        /// <param name="isRunningFunc">A method that receives the console output of the executable to check when it was finished starting</param>
        /// <param name="workDirectory">Optionally a working directory for the executable can be specified</param>
        /// <param name="executableArguments">Optionally environment variables can be specified for the executable</param>
        /// <param name="environmentVariables">Optionally environment variables can be specified for the executable</param>
        public ExecutableRunner(string executablePath, Func<string?, bool> isRunningFunc, string? workDirectory = null, List<string>? executableArguments = null, IDictionary<string, string>? environmentVariables = null)
        {
            _executablePath = executablePath;
            _runWorkDirectory = workDirectory;
            _isRunningFunc = isRunningFunc;
            _executableArguments = executableArguments;
            _executableEnvironmentVariables = environmentVariables;
        }

        /// <summary>
        /// Provides a way to set the build configuration so that the application can be ran in a sandbox,
        /// Such as editing application configurations files or even source code before building it.
        /// </summary>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureBuild(Func<Task> configureBuildSandboxFunc)
        {
            if (_configureBuildSandboxFunc != null)
            {
                throw new InvalidOperationException("ConfigureBuildFunc already set.");
            }
            _configureBuildSandboxFunc = configureBuildSandboxFunc;
        }

        /// <summary>
        /// Configures a method to run to reset the running executable.
        /// </summary>
        /// <param name="resetFunc"></param>
        public void OnConfigureReset(Func<Process, Task> resetFunc)
        {
            _resetFunc = resetFunc;
        }

        /// <summary>
        /// Builds the executable or related things if needed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task BuildAsync()
        {
            if (_executableBuildPath == null)
            {
                return;
            }
            _isDoneBuildingTaskCompletionSource = new TaskCompletionSource<bool>();
            var buildProcessStartupInfo = new ProcessStartInfo
            {
                FileName = _executableBuildPath,
            };
            buildProcessStartupInfo.RedirectStandardOutput = true;
            buildProcessStartupInfo.RedirectStandardInput = true;
            buildProcessStartupInfo.RedirectStandardError = true;
            buildProcessStartupInfo.WorkingDirectory = _buildWorkDirectory;
            if (_buildArguments != null)
            {
                foreach (var arg in _buildArguments) 
                {
                    buildProcessStartupInfo.ArgumentList.Add(arg);
                }
            }
            if (_buildEnvironmentVariables != null)
            {
                foreach (var environmentVariable in _buildEnvironmentVariables)
                {
                    buildProcessStartupInfo.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
                }
            }
            _buildProcess = new Process();
            _buildProcess.StartInfo = buildProcessStartupInfo;
            _buildProcess.Exited += _buildProcess_Exited;
            _buildProcess.Start();
            var result = await _isDoneBuildingTaskCompletionSource.Task;
            if (result == false)
            {
                throw new InvalidOperationException("Failed to build the executable");
            }
        }

        private void _buildProcess_Exited(object? sender, EventArgs e)
        {
            if (_buildProcess!.ExitCode != 1 && _buildProcess!.ExitCode != 0)
            {
                _isDoneBuildingTaskCompletionSource!.SetResult(false);
            }
            _isDoneBuildingTaskCompletionSource!.SetResult(true);
        }

        /// <summary>
        /// Provides a way to set any remaining configurations that can only be done after the application is built in order to run
        /// it in a sandbox such as editing application configuration files from the executable's directory.
        /// </summary>
        /// <param name="configureRunFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureRun(Func<Task>? configureRunFunc)
        {
            if (_configureRunSandboxFunc != null)
            {
                throw new InvalidOperationException("Configure run function already set.");
            }
            _configureRunSandboxFunc = configureRunFunc;
        }

        ///<inheritdoc/>
        public async virtual Task ConfigureBuildAsync()
        {
            if (_configureRunSandboxFunc != null)
            {
                await _configureRunSandboxFunc();
            }
        }

        ///<inheritdoc/>
        public async virtual Task ConfigureRunAsync()
        {
            if (_configureRunSandboxFunc != null)
            {
                await _configureRunSandboxFunc();
            }
        }

        ///<inheritdoc/>
        public async virtual Task ResetAsync()
        {
            if (_executableProcess == null)
            {
                throw new InvalidOperationException("Executable is not running");
            }
            if (_resetFunc != null)
            {
                await _resetFunc(_executableProcess);
            }
        }

        ///<inheritdoc/>
        public async virtual Task RunAsync()
        {
            _isRunningTaskCompletionSource = new TaskCompletionSource<bool>();
            var executableProcessStartupInfo = new ProcessStartInfo
            {
                FileName = _executableBuildPath,
            };
            executableProcessStartupInfo.RedirectStandardOutput = true;
            executableProcessStartupInfo.RedirectStandardInput = true;
            executableProcessStartupInfo.RedirectStandardError = true;
            executableProcessStartupInfo.WorkingDirectory = _buildWorkDirectory;
            if (_executableArguments != null)
            {
                foreach (var arg in _executableArguments)
                {
                    executableProcessStartupInfo.ArgumentList.Add(arg);
                }
            }
            if (_executableEnvironmentVariables != null)
            {
                foreach (var environmentVariable in _executableEnvironmentVariables)
                {
                    executableProcessStartupInfo.EnvironmentVariables[environmentVariable.Key] = environmentVariable.Value;
                }
            }
            _executableProcess = new Process();
            _executableProcess.StartInfo = executableProcessStartupInfo;
            _executableProcess.Exited += _executableProcess_Exited;
            _executableProcess.OutputDataReceived += _executableProcess_OutputDataReceived;
            _executableProcess.Start();
            var runResult = await _isRunningTaskCompletionSource.Task;
            if (runResult == false)
            {
                throw new InvalidOperationException("Failed to start the executable");
            }
        }

        private void _executableProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_isRunningFunc(e.Data))
            {
                _isRunningTaskCompletionSource!.SetResult(true);
            }
        }

        private void _executableProcess_Exited(object? sender, EventArgs e)
        {
            _isRunningTaskCompletionSource!.SetResult(false);
        }

        ///<inheritdoc/>
        public virtual async Task StopAsync()
        {
            if (_executableProcess == null)
            {
                throw new InvalidOperationException("Executable not started");
            }
            _executableProcess.Exited -= _executableProcess_Exited;
            _executableProcess.Close();
            await _executableProcess.WaitForExitAsync();
        }
    }
}
