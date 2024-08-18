using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Represents a runner capable of running generic .Net core hosts <see cref="IHost"/>.
    /// </summary>
    public class HostRunner : RunnerBase, IHostRunner
    {
        protected IHost? _host;
        protected IHostBuilder? _hostBuilder;
        protected Func<Task<IHostBuilder>> _hostBuilderFunc;
        protected Func<IHostBuilder, Task>? _configureBuildFunc;
        protected Func<IHost, Task>? _configureRunFunc;
        protected Func<IHost, Task>? _resetFunc;
        protected string[]? _arguments;

        ///<inheritdoc/>
        public IHost Host => _host ?? throw new InvalidOperationException("Host runner is not built");

        ///<inheritdoc/>
        public IHostBuilder HostBuilder => _hostBuilder ?? throw new InvalidOperationException("Host builder is not found");

        /// <summary>
        /// Creates a new instance of <see cref="HostRunner"/> having as parameter a function that return the original host builder.
        /// </summary>
        /// <param name="hostBuilderSourceFunc"></param>
        public HostRunner(Func<Task<IHostBuilder>> hostBuilderSourceFunc)
        {
            _hostBuilderFunc = hostBuilderSourceFunc;
        }

        /// <summary>
        /// Builds the host by calling the IHostBuilder.Build method.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task BuildAsync(IScenarioSuiteContext scenarioSuiteContext)
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
        /// <param name="configureBuildFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureBuild(Func<IHostBuilder, Task> configureBuildFunc)
        {
            if (_configureBuildFunc != null)
            {
                throw new InvalidOperationException("ConfigureBuildFunc already set.");
            }
            _configureBuildFunc = configureBuildFunc;
        }

        /// <summary>
        /// Use the configure sandbox function to allow the host to run in a sandbox.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureBuildAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _hostBuilder = await _hostBuilderFunc();
            if (_hostBuilder == null)
            {
                throw new InvalidOperationException("Host builder not found.");
            }
            if (_configureBuildFunc != null)
            {
                await _configureBuildFunc(_hostBuilder);
            }
        }

        /// <summary>
        /// Uses the configure run function to perform any additional operations after building the application but before running it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureRunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built.");
            }
            if (_configureRunFunc != null)
            {
                await _configureRunFunc(_host);
            }
        }

        /// <summary>
        /// Provides a way to set any remaining configurations that can only be done after the application is built in order to run
        /// it in a sandbox.
        /// </summary>
        /// <param name="configureRunFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureRun(Func<IHost, Task>? configureRunFunc)
        {
            if (_configureRunFunc != null)
            {
                throw new InvalidOperationException("Configure run function already set.");
            }
            _configureRunFunc = configureRunFunc;
        }

        ///<inheritdoc/>
        public override async Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built.");
            }
            await _host.StartAsync();
            _isRunning = true;
        }

        ///<inheritdoc/>
        public override async Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Host not built and is not running.");
            }
            await _host.StopAsync();
            _host.Dispose();
            _isRunning = false;
        }

        ///<inheritdoc/>
        public override async Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
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
