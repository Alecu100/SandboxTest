using SandboxTest.Instance;
using System.Diagnostics;

namespace SandboxTest.Node
{
    public class NodeRunner : RunnerBase, INodeRunner
    {
        private Process? _nodeProcess;

        private Func<string, bool>? _readyFunc;

        private Func<string, bool>? _errorFunc;

        private string _host;

        private int _port;

        private bool _useSsl;

        private string _url;

        private Func<Task>? _configureBuildFunc;

        private Func<Task>? _configureRunFunc;

        public NodeRunner() 
        {
            _host = "localhost";
            _port = 80;
            _useSsl = true;
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
        public Task BuildAsync()
        {
            throw new NotImplementedException();
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

        /// <summary>
        /// Configures the parameters to run the node server with.
        /// </summary>
        /// <param name="host">The host on which to start the node server.</param>
        /// <param name="port">The port to use for the node server.</param>
        /// <param name="useSsl">Enables the node server to use ssl</param>
        public void OnConfigureNode(string host, int port, bool useSsl)
        {
            _port = port;
            _host = host;
            _useSsl = useSsl;
            _url = $"{(_useSsl ? "https" : "http")}://{_host}:{_port}";
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
        public override Task RunAsync()
        {
            return Task.CompletedTask;
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
