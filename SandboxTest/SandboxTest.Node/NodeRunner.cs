using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SandboxTest.Node
{
    public class NodeRunner : RunnerBase, INodeRunner
    {
        private static bool NodeFound;
        private static readonly string? NodeExecutableName;
        private static readonly string? NodePath;
        private const string NodeCliPath = "node_modules\\npm\\bin\\npm-cli.js";
        private const string Listening = "LISTENING";
        private const string Listen = "LISTEN";
        private const string Node = "node";

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

        static NodeRunner()
        {
            NodeExecutableName = Environment.OSVersion.Platform == PlatformID.Win32NT ? $"{Node}.exe" : Node;
            var cancellationTokenSource = new CancellationTokenSource(20000);
            try
            {
                var nodeProcess = Process.Start(NodeExecutableName);
                while (nodeProcess.MainModule == null || cancellationTokenSource.IsCancellationRequested)
                {

                }
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    NodePath = Path.GetDirectoryName(nodeProcess!.MainModule!.FileName)!;
                    nodeProcess.Kill(true);
                    NodeFound = true;
                }
                else
                {
                    NodeFound = false;
                }
            }
            catch(Exception)
            {
                NodeFound = false;
            }
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
        public async Task BuildAsync(IScenarioSuiteContext scenarioSuiteContext)
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
        public async Task ConfigureBuildAsync(IScenarioSuiteContext scenarioSuiteContext)
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
        public async Task ConfigureRunAsync(IScenarioSuiteContext scenarioSuiteContext)
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
        public override Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (NodeFound == false)
            {
                throw new InvalidOperationException("Node.js not found");
            }
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
            _isRunning = true;
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
        public override async Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (NodeFound == false)
            {
                throw new InvalidOperationException("Node.js not found");
            }
            if (_nodeProcess == null)
            {
                throw new InvalidOperationException("Node server not started");
            }
            if (!_nodeProcess.HasExited)
            {
                _nodeProcess.Kill(true);
            }

            int remainingNodeProcessId = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var windowsCommandResult = await CommandLineUtils.RunCommandAsync($"netstat -ano | find \"{Listening}\" | find \"{_port}\"");
                var listeningLastIndex = windowsCommandResult.LastIndexOf(Listening, StringComparison.InvariantCultureIgnoreCase);
                if (listeningLastIndex > 0)
                {
                    remainingNodeProcessId = int.Parse(windowsCommandResult.Substring(listeningLastIndex + Listening.Length).Trim());
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var linuxCommandResult = await CommandLineUtils.RunCommandAsync($"netstat -nlp | grep :{_port}");
                var listenLastIndex = linuxCommandResult.LastIndexOf(Listen);
                if (listenLastIndex > 0)
                {
                    remainingNodeProcessId = int.Parse(linuxCommandResult.Substring(listenLastIndex + Listening.Length).Split('/')[0].Trim());
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var macCommandResult = await CommandLineUtils.RunCommandAsync($"lsof -Pi :{_port}");
                var nodeLastIndex = macCommandResult.LastIndexOf(Node);
                if (nodeLastIndex > 0)
                {
                    remainingNodeProcessId = int.Parse(macCommandResult.Split('\n')[1].Trim('\r', ' ').Split('\t', ' ', StringSplitOptions.RemoveEmptyEntries)[1]);
                }
            }

            if (remainingNodeProcessId != 0)
            {
                var remainingNodeProcess = Process.GetProcessById(remainingNodeProcessId);
                remainingNodeProcess.Kill(true);
                await remainingNodeProcess.WaitForExitAsync();
            }
            _isRunning = false;
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
            process.Close();

            return output.ToString();
        }

        private static Process RunNodeProcess(string commandToRun, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            return RunNodeProcess(commandToRun, Environment.CurrentDirectory, outputReceived, errorReceived);
        }

        private static Process RunNodeProcess(string commandToRun, string workingDirectory, Action<string>? outputReceived = null, Action<string>? errorReceived = null)
        {
            var commandLineProcess = new Process();
            commandLineProcess.StartInfo.FileName = Path.Combine(NodePath!, NodeExecutableName!);
            commandLineProcess.StartInfo.WorkingDirectory = workingDirectory;
            commandLineProcess.StartInfo.UseShellExecute = false;
            commandLineProcess.StartInfo.RedirectStandardError = true;
            commandLineProcess.StartInfo.RedirectStandardOutput = true;
            commandLineProcess.StartInfo.RedirectStandardInput = true;
            commandLineProcess.StartInfo.Arguments = $"\"{Path.Combine(NodePath!, NodeCliPath)}\" {commandToRun}";
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
