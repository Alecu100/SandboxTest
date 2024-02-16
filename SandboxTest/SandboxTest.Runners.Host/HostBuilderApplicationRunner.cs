
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;

namespace SandboxTest.Runners.Host
{
    public class HostBuilderApplicationRunner : IApplicationRunner, IHostApplicationRunner
    {
        private IHost? _host;
        private IHostBuilder? _hostBuilder;
        private Func<string[], Task<IHostBuilder>> _hostBuilderFunc;
        private Func<IHostBuilder, Task>? _configureBuildSandboxFunc;
        private Func<IHost, Task>? _configureRunSandboxFunc;

        public IHost Host => _host ?? throw new InvalidOperationException("Host is not built.");
        public IHostBuilder HostBuilder => _hostBuilder ?? throw new InvalidOperationException("HostBuilder is not set up.");

        public HostBuilderApplicationRunner(Func<string[], Task<IHostBuilder>> hostBuilderSourceFunc)
        {
            _hostBuilderFunc = hostBuilderSourceFunc;
        }

        public Task BuildAsync()
        {
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("HostBuilder not found.");
            }
            _host = _hostBuilder.Build();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Provides a way to set the host builder configuration so that the application can be ran in a sandbox,
        /// Such as replacing sql server database context with sqlLite in memory database context
        /// </summary>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureBuildSandbox(Func<IHostBuilder, Task> configureBuildSandboxFunc)
        {
            if (_configureBuildSandboxFunc != null)
            {
                throw new InvalidOperationException("ConfigureBuildFunc already set.");
            }
            _configureBuildSandboxFunc = configureBuildSandboxFunc;
        }

        public async Task ConfigureBuildAsync()
        {
            _hostBuilder = await _hostBuilderFunc([]);
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("HostBuilder not found.");
            }
            if (_configureBuildSandboxFunc != null && _hostBuilder != null)
            {
                await _configureBuildSandboxFunc(_hostBuilder);
            }
        }

        public async Task ConfigureRunAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built.");
            }
            if (_configureRunSandboxFunc != null)
            {
                await _configureRunSandboxFunc(_host);
            }
        }

        /// <summary>
        /// Provides a way to set any remaining configurations that can only be done after the application is built in order to run
        /// it in a sandbox.
        /// </summary>
        /// <param name="configureRunFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureRunSandbox(Func<IHost, Task>? configureRunFunc)
        {
            if (_configureRunSandboxFunc != null)
            {
                throw new InvalidOperationException("ConfigureRunFunc already set.");
            }
            _configureRunSandboxFunc = configureRunFunc;
        }

        public async Task RunAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built.");
            }
            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built and is not running.");
            }
            await _host.StopAsync();
        }
    }
}
