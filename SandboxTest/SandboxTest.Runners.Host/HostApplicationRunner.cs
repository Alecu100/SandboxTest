
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;

namespace SandboxTest.Runners.Host
{
    public class HostApplicationRunner : IApplicationRunner, IHostApplicationRunner
    {
        private IHost? _host;
        private IHostBuilder? _hostBuilder;
        private Func<string[], Task<IHostBuilder>> _hostBuilderFunc;
        private Func<IHostBuilder, Task>? _configureBuildSandboxFunc;
        private Func<IHost, Task>? _configureRunSandboxFunc;
        private Func<IHost, Task>? _resetFunc;

        ///<inheritdoc/>
        public IHost Host => _host ?? throw new InvalidOperationException("Host application runner is not built");

        ///<inheritdoc/>
        public IHostBuilder HostBuilder => _hostBuilder ?? throw new InvalidOperationException("Host application runner is not built");

        public HostApplicationRunner(Func<string[], Task<IHostBuilder>> hostBuilderSourceFunc)
        {
            _hostBuilderFunc = hostBuilderSourceFunc;
        }

        /// <summary>
        /// Builds the host by calling the IHostBuilder.Build method.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Use the configure sandbox function to allow the host to run in a sandbox.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ConfigureBuildAsync()
        {
            _hostBuilder = await _hostBuilderFunc([]);
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("HostBuilder not found.");
            }
            if (_configureBuildSandboxFunc != null)
            {
                await _configureBuildSandboxFunc(_hostBuilder);
            }
        }

        /// <summary>
        /// Uses the configure run function.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

        /// <summary>
        /// Provides a way to clean up an IHost before starting another scenario from the same scenarion container.
        /// </summary>
        /// <param name="resetFunc"></param>
        public void OnConfigureReset(Func<IHost, Task>? resetFunc)
        {
            _resetFunc = resetFunc;
        }

        ///<inheritdoc/>
        public async Task RunAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built.");
            }
            await _host.StartAsync();
        }

        ///<inheritdoc/>
        public async Task StopAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built and is not running.");
            }
            await _host.StopAsync();
        }

        ///<inheritdoc/>
        public async Task ResetAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built and is not running.");
            }
            if (_resetFunc != null)
            {
                await _resetFunc(_host);
            }
        }
    }
}
