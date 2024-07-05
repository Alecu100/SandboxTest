using SandboxTest.Instance;
using SandboxTest.Utils;
using System.Diagnostics;

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

        /// <param name="sourcePath">The path of the sources.</param>
        /// <param name="host">The host on which to start the node server.</param>
        /// <param name="port">The port to use for the node server.</param>
        /// <param name="useSsl">Enables the node server to use ssl</param>
        public NodeRunner(string host = "localhost", int port = 80, bool useSsl = false) 
        {
            _host = host;
            _port = port;
            _useSsl = useSsl;
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

            await CommandLineUtils.RunCommand($"npm install -f" , _sourcePath);
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
            _nodeProcess = await CommandLineUtils.RunProcess($"npm {_npmRunCommand} -- --host \"{_host}\" --port {_port}", _sourcePath, (output) =>
            {
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

        /// <inheritdoc/>
        public override Task StopAsync()
        {
            if (_nodeProcess == null)
            {
                throw new InvalidOperationException("Node server not started");
            }
            if (!_nodeProcess.HasExited)
            {
                _nodeProcess.Kill(true);
            }
            return Task.CompletedTask;
        }
    }
}
