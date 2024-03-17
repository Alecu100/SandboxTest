using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Represents a runner capable of running generic .Net core hosts <see cref="IHost"/>.
    /// </summary>
    public class HostApplicationRunner : IApplicationRunner, IHostApplicationRunner
    {
        protected IHost? _host;
        protected IHostBuilder? _hostBuilder;
        protected Func<string[], Task<IHostBuilder>> _hostBuilderFunc;
        protected Func<IHostBuilder, Task>? _configureBuildSandboxFunc;
        protected Func<IHost, Task>? _configureRunSandboxFunc;
        protected Func<IHost, Task>? _resetFunc;
        protected string[]? _arguments;

        ///<inheritdoc/>
        public IHost Host => _host ?? throw new InvalidOperationException("Host application runner is not built");

        ///<inheritdoc/>
        public IHostBuilder HostBuilder => _hostBuilder ?? throw new InvalidOperationException("Host builder is not found");

        /// <summary>
        /// Creates a new instance of <see cref="HostApplicationRunner"/> having as parameter a function that return the original host builder.
        /// </summary>
        /// <param name="hostBuilderSourceFunc"></param>
        public HostApplicationRunner(Func<string[], Task<IHostBuilder>> hostBuilderSourceFunc)
        {
            _hostBuilderFunc = hostBuilderSourceFunc;
        }

        /// <summary>
        /// Builds the host by calling the IHostBuilder.Build method.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task BuildAsync()
        {
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("Host builder not found.");
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
        /// Configures the arguments to use when creating the <see cref="Microsoft.Extensions.Hosting.HostBuilder"/>
        /// </summary>
        /// <param name="arguments"></param>
        public void OnConfigureArguments(params string[] arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Use the configure sandbox function to allow the host to run in a sandbox.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureBuildAsync()
        {
            _hostBuilder = await _hostBuilderFunc(_arguments ?? []);
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("Host builder not found.");
            }
            if (_configureBuildSandboxFunc != null)
            {
                await _configureBuildSandboxFunc(_hostBuilder);
            }
        }

        /// <summary>
        /// Uses the configure run function to perform any additional operations after building the application but before running it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureRunAsync()
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
                throw new InvalidOperationException("Configure run function already set.");
            }
            _configureRunSandboxFunc = configureRunFunc;
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
        public virtual async Task StopAsync()
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built and is not running.");
            }
            await _host.StopAsync();
        }

        ///<inheritdoc/>
        public virtual async Task ResetAsync()
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

        /// <summary>
        /// Provides a way to clean up an IHost before starting another scenario from the same scenario suite.
        /// </summary>
        /// <param name="resetFunc"></param>
        public void OnConfigureReset(Func<IHost, Task>? resetFunc)
        {
            _resetFunc = resetFunc;
        }
    }
}
