using SandboxTest.Instance;
using SandboxTest.Utils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SandboxTest.Node
{
    public class NodeRunner : RunnerBase, INodeRunner
    {
        private Process? _nodeProcess;

        private Func<string, bool>? _parseReadyFunc;

        private Func<string, bool>? _parseErrorFunc;

        private string _host;

        private int _port;

        private bool _useSsl;

        private string _url;

        private Func<Task>? _configureBuildFunc;

        private Func<Task>? _configureRunFunc;

        private string? _sourcePath;

        private string? _npmRunCommand;

        private TaskCompletionSource<bool>? _runCompletionSource;

        private static readonly string NodeExecutableName;
        private static readonly string NodePath;
        private static readonly string NodeCliPath = "node_modules\\npm\\bin\\npm-cli.js";

        static NodeRunner()
        {
            var path = Environment.ExpandEnvironmentVariables("node.exe");
            NodeExecutableName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "node.exe" : "node";
            var nodeProcess = Process.Start(NodeExecutableName);
            NodePath = Path.GetDirectoryName(nodeProcess!.MainModule!.FileName)!;
            nodeProcess.Kill(true);
        }

        /// <param name="sourcePath">The path of the sources.</param>
        /// <param name="host">The host on which to start the node server.</param>
        /// <param name="port">The port to use for the node server.</param>
        /// <param name="useSsl">Enables the node server to use ssl</param>
        public NodeRunner(string host = "localhost", int port = 80, bool useSsl = true) 
        {
            _host = host;
            _port = port;
            _useSsl = useSsl;
            _url = string.Empty;
            RefreshUrl();
        }

        private void RefreshUrl()
        {
            _url = $"{(_useSsl ? "https" : "http")}://{_host}:{_port}";
        }

        /// <inheritdoc/>
        public string Host => _host;

        /// <inheritdoc/>
        public int Port => _port;

        /// <inheritdoc/>
        public string Url => _url;

        public bool UseSssl => _useSsl;

        /// <inheritdoc/>
        public async Task BuildAsync()
        {
            if (_parseErrorFunc == null || _parseReadyFunc == null || _sourcePath == null || _npmRunCommand == null)
            {
                throw new InvalidOperationException("Node run not cofigured");
            }

            await RunNodeCommandAsync($"install -f" , _sourcePath);
        }

        public void OnConfigureNode(Func<string, bool> parseReadyFunc, Func<string, bool>? parseErrorFunc, string sourcePath, string npmRunCommand)
        {
            _sourcePath = sourcePath;
            _parseReadyFunc = parseReadyFunc;
            _parseErrorFunc = parseErrorFunc;
            _npmRunCommand = npmRunCommand;
        }

        /// <inheritdoc/>
        public async Task ConfigureBuildAsync()
        {
            if (_configureBuildFunc != null)
            {
                await _configureBuildFunc();
            }
        }

        public void OnConfigureBuild(Func<Task> configureBuildFunc)
        {
            _configureBuildFunc = configureBuildFunc;
        }

        public void OnConfigureRun(Func<Task> configureRunFunc)
        {
            _configureRunFunc = configureRunFunc;
        }

        /// <inheritdoc/>
        public async Task ConfigureRunAsync()
        {
            if (_configureRunFunc != null)
            {
                await _configureRunFunc();
            }
        }

        /// <summary>
        /// Node instance can't be reset.
        /// </summary>
        /// <returns></returns>
        public override Task ResetAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task RunAsync()
        {
            if (_parseErrorFunc == null || _parseReadyFunc == null || _sourcePath == null)
            {
                throw new InvalidOperationException("Parse error func and parse ready func not set");
            }

            _runCompletionSource = new TaskCompletionSource<bool>();
            _nodeProcess = RunNodeProcess($"{_npmRunCommand} -- --host \"{_host}\" --port {_port}", _sourcePath, (output) =>
            {
                if (ParsePortIsInUse(output))
                {
                    _port += 1;
                    RefreshUrl();
                }
                if (_parseErrorFunc(output))
                {
                    _runCompletionSource.SetResult(false);
                }
                if (_parseReadyFunc(output))
                {
                    _runCompletionSource.SetResult(true);
                }
            });
            await _runCompletionSource.Task;
        }

        private bool ParsePortIsInUse(string output)
        {
            var indexOfPort = output.IndexOf("port", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfPort < 0)
            {
                return false;
            }
            var indexOfIsInUse = output.IndexOf("is in use, trying another one", indexOfPort, StringComparison.InvariantCultureIgnoreCase);
            if (indexOfIsInUse < 0)
            {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override Task StopAsync()
        {
            if (_nodeProcess == null)
            {
                throw new InvalidOperationException("Node server not started");
            }
            if (!_nodeProcess.HasExited)
            {
                _nodeProcess.CancelErrorRead();
                _nodeProcess.CancelOutputRead();
                _nodeProcess.Close();
            }
            _nodeProcess.WaitForExitAsync();

            return Task.CompletedTask;
        }

        private static async Task<string> RunNodeCommandAsync(string commandToRun)
        {
            var process = RunNodeProcess(commandToRun, Environment.CurrentDirectory);

            var output = new StringBuilder();
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs args) {
                output.AppendLine(args.Data);
            };
            process.BeginOutputReadLine();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync();

            return output.ToString();
        }

        private static async Task<string> RunNodeCommandAsync(string commandToRun, string workingDirectory)
        {
            var output = new StringBuilder();
            var process = RunNodeProcess(commandToRun, workingDirectory, text => output.AppendLine(text));

            await process.WaitForExitAsync();

            return output.ToString();
        }

        private static Process RunNodeProcess(string commandToRun, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            return RunNodeProcess(commandToRun, Environment.CurrentDirectory, outputReceived, errorReceived);
        }

        private static Process RunNodeProcess(string commandToRun, string workingDirectory, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            var commandLineProcess = new Process();
            commandLineProcess.StartInfo.FileName = Path.Combine(NodePath, NodeExecutableName);
            commandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
            commandLineProcess.StartInfo.UseShellExecute = false;
            commandLineProcess.StartInfo.RedirectStandardError = true;
            commandLineProcess.StartInfo.RedirectStandardOutput = true;
            commandLineProcess.StartInfo.RedirectStandardInput = true;
            commandLineProcess.StartInfo.Arguments = $"\"{Path.Combine(NodePath, NodeCliPath)}\" {commandToRun}";
            commandLineProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            if (outputReceived != null)
            {
                commandLineProcess.OutputDataReceived += (sender, args) =>
                {
                    outputReceived(args.Data ?? string.Empty);
                };
            }
            if (errorReceived != null)
            {
                commandLineProcess.ErrorDataReceived += (sender, args) =>
                {
                    errorReceived(args.Data ?? string.Empty);
                };
            }
            commandLineProcess.Start();
            commandLineProcess.BeginOutputReadLine();
            commandLineProcess.BeginErrorReadLine();

            return commandLineProcess;
        }
    }
}
