using SandboxTest;
using SandboxTest.Runners.Executables;
using System.Diagnostics;

namespace SanboxTest.Runners.Executables
{
    /// <summary>
    /// Represents an runner that actually starts another executable as a child process, capturing it's outputs and inputs.
    /// </summary>
    public class ExecutableApplicationRunner : IApplicationRunner, IExecutableApplicationRunner
    {
        protected Process? _executableProcess;
        protected Process? _buildProcess;
        protected TaskCompletionSource<bool>? _isDoneBuildingTaskCompletionSource;
        protected Func<string?, bool>? _isRunningFunc;
        protected string? _buildCommand;
        protected List<string>? _buildArguments;
        protected string? _buildWorkDirectory;
        protected string? _runCommand;
        protected List<string>? _runArguments;
        protected string? _runWorkDirectory;
        protected IDictionary<string, string>? _executableEnvironmentVariables;

        public Process ExecutableProcess => _executableProcess ?? throw new InvalidOperationException("Executable not started");

        /// <summary>
        /// Configures how what executable to run and how to run it.
        /// </summary>
        /// <param name="runCommand">The actual command to run the executable</param>
        /// <param name="runCommand">The actual command to run the executable</param>
        /// <param name="isRunningFunc">A method that receives the console output of the executable to check when it was finished starting</param>
        /// <param name="workDirectory">Optionally a working directory for the executable can be specified</param>
        /// <param name="environmentVariables">Optionally environment variables can be specified for the executable</param>
        public void ConfigureExecutableRun(string runCommand, Func<string?, bool> isRunningFunc, List<string>? runArguments = null, string? workDirectory = null, IDictionary<string, string>? environmentVariables = null)
        {
            _runCommand = runCommand;
            _runArguments = runArguments;
            _runWorkDirectory = workDirectory;
            _isRunningFunc = isRunningFunc;
            _executableEnvironmentVariables = environmentVariables;
        }


        /// <summary>
        /// Optionally configures building of the executable or related things.
        /// </summary>
        /// <param name="buildCommand"></param>
        /// <param name="isDoneBuildingFunc"></param>
        /// <param name="buildArguments"></param>
        public void ConfigureExecutableBuild(string buildCommand, List<string>? buildArguments, string? workDirectory)
        {
            _buildCommand = buildCommand;
            _buildArguments = buildArguments;
            _buildWorkDirectory = workDirectory;
        }

        /// <summary>
        /// Builds the executable or related things if needed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task BuildAsync()
        {
            if (_buildCommand == null)
            {
                return;
            }
            _isDoneBuildingTaskCompletionSource = new TaskCompletionSource<bool>();
            var buildProcessStartupInfo = new ProcessStartInfo
            {
                FileName = _buildCommand
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
            _buildProcess = new Process();
            _buildProcess.StartInfo = buildProcessStartupInfo;
            _buildProcess.Exited += _buildProcess_Exited;
            _buildProcess.Start();
            await _isDoneBuildingTaskCompletionSource.Task;
        }

        private void _buildProcess_Exited(object? sender, EventArgs e)
        {
            if (_buildProcess!.ExitCode != 1 && _buildProcess!.ExitCode != 0)
            {
                _isDoneBuildingTaskCompletionSource!.SetResult(false);
            }
            _isDoneBuildingTaskCompletionSource!.SetResult(true);
        }

        public virtual Task ConfigureBuildAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task ConfigureRunAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task ResetAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task RunAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
