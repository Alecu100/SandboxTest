using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;

namespace SandboxTest.AspNetCore
{
    /// <summary>
    /// Represents a runner capable of running Asp .Net Core web applications <see cref="WebApplication"/>.
    /// </summary>
    public class WebApplicationRunner : IApplicationRunner, IHostApplicationRunner
    {
        protected WebApplicationBuilder? _webApplicationBuilder;
        protected WebApplication? _webApplication;
        protected Func<string[], Task<WebApplicationBuilder>> _webApplicationBuilderFunc;
        protected Func<WebApplicationBuilder, Task>? _configureBuildSandboxFunc;
        protected Func<WebApplication, Task>? _configureRunSandboxFunc;
        protected Func<WebApplication, Task>? _resetFunc;
        protected string[]? _arguments;
        protected Task? _runTask;
        protected string? _url;

        public IHost Host => _webApplication ?? throw new InvalidOperationException("Web application runner not built");

        public IHostBuilder HostBuilder => _webApplicationBuilder?.Host ?? throw new InvalidOperationException("Web application builder is not found");

        /// <summary>
        /// Creates a new instance of <see cref="WebApplicationRunner"/> having as parameter a function that return the original web application builder.
        /// </summary>
        /// <param name="webApplicationBuilderFunc"></param>
        public WebApplicationRunner(Func<string[], Task<WebApplicationBuilder>> webApplicationBuilderFunc)
        {
            _webApplicationBuilderFunc = webApplicationBuilderFunc;
        }

        /// <summary>
        /// Provides a way to set the web aplication builder configuration so that the web application can be ran in a sandbox,
        /// Such as replacing sql server database context with sqlLite in memory database context.
        /// </summary>
        /// <param name="configureBuildSandboxFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureBuildSandbox(Func<WebApplicationBuilder, Task> configureBuildSandboxFunc)
        {
            if (_configureBuildSandboxFunc != null)
            {
                throw new InvalidOperationException("ConfigureBuildFunc already set.");
            }
            _configureBuildSandboxFunc = configureBuildSandboxFunc;
        }

        /// <summary>
        /// Configures the arguments to use when creating the <see cref="WebApplicationBuilder"/>
        /// </summary>
        /// <param name="arguments"></param>
        public void OnConfigureArguments(params string[] arguments)
        {
            _arguments = arguments;
        }

        /// <summary>
        /// Configures the url to start the web application on.
        /// </summary>
        /// <param name="url"></param>
        public void OnConfigureUrl(string url)
        {
            _url = url;
        }

        /// <summary>
        /// Builds and creates the web application
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task BuildAsync()
        {
            if (_webApplicationBuilder == null)
            {
                throw new InvalidOperationException("WebApplicationBuilder not found.");
            }
            _webApplication = _webApplicationBuilder.Build();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Use the configure sandbox function to allow the host to run in a sandbox.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureBuildAsync()
        {
            _webApplicationBuilder = await _webApplicationBuilderFunc(_arguments ?? []);
            if (_webApplicationBuilder == null)
            {
                throw new InvalidOperationException("Web application builder not found.");
            }
            if (_configureBuildSandboxFunc != null)
            {
                await _configureBuildSandboxFunc(_webApplicationBuilder);
            }
        }

        /// <summary>
        /// Uses the configure run function to perform any additional operations after building the web application but before running it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ConfigureRunAsync()
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built.");
            }
            if (_configureRunSandboxFunc != null)
            {
                await _configureRunSandboxFunc(_webApplication);
            }
        }

        /// <summary>
        /// Provides a way to set any remaining configurations that can only be done after the web application is built in order to run
        /// it in a sandbox.
        /// </summary>
        /// <param name="configureRunFunc"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void OnConfigureRunSandbox(Func<WebApplication, Task>? configureRunFunc)
        {
            if (_configureRunSandboxFunc != null)
            {
                throw new InvalidOperationException("Configure run function already set.");
            }
            _configureRunSandboxFunc = configureRunFunc;
        }

        ///<inheritdoc/>
        public virtual async Task ResetAsync()
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built and is not running.");
            }
            if (_resetFunc != null)
            {
                await _resetFunc(_webApplication);
            }
        }

        /// <summary>
        /// Provides a way to clean up an IHost before starting another scenario from the same scenario suite.
        /// </summary>
        /// <param name="resetFunc"></param>
        public void OnConfigureReset(Func<WebApplication, Task>? resetFunc)
        {
            _resetFunc = resetFunc;
        }

        /// <summary>
        /// Starts the web application on the configured address.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task RunAsync()
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built");
            }
            _runTask = Task.Run(async () => await _webApplication.RunAsync(_url));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the running web application
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task StopAsync()
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built and is not running.");
            }
            await _webApplication.StopAsync();
        }
    }
}
