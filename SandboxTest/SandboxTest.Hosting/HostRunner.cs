using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
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
        /// Used to initialize the builder for the host.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeBuilderAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _hostBuilder = await _hostBuilderFunc();
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
