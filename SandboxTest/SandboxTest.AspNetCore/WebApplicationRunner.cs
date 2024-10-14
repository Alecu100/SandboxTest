﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.AspNetCore
{
    /// <summary>
    /// Represents a runner capable of running Asp .Net Core web applications <see cref="WebApplication"/>.
    /// </summary>
    public class WebApplicationRunner : RunnerBase, IWebApplicationRunner
    {
        protected WebApplicationBuilder? _webApplicationBuilder;
        protected WebApplication? _webApplication;
        protected Func<Task<WebApplicationBuilder>> _webApplicationBuilderFunc;
        protected Func<WebApplicationBuilder, Task>? _configureBuildFunc;
        protected Func<WebApplication, Task>? _configureRunFunc;
        protected Func<WebApplication, Task>? _resetFunc;
        protected Task? _runTask;
        protected string _url;

        ///<inheritdoc/>
        public IHost Host => _webApplication ?? throw new InvalidOperationException("Web application runner not built");

        ///<inheritdoc/>
        public IHostBuilder HostBuilder => _webApplicationBuilder?.Host ?? throw new InvalidOperationException("Web application builder is not found");

        ///<inheritdoc/>
        public WebApplication WebApplication => _webApplication ?? throw new InvalidOperationException("Web application runner not built");

        ///<inheritdoc/>
        public WebApplicationBuilder WebApplicationBuilder => _webApplicationBuilder ?? throw new InvalidOperationException("Web application builder is not found");

        ///<inheritdoc/>
        public string Url => _url ?? throw new InvalidOperationException("Web application runner not built");

        /// <summary>
        /// Creates a new instance of <see cref="WebApplicationRunner"/> having as parameter a function that return the original web application builder.
        /// </summary>
        /// <param name="webApplicationBuilderFunc"></param>
        public WebApplicationRunner(Func<Task<WebApplicationBuilder>> webApplicationBuilderFunc, string url)
        {
            _url = url;
            _webApplicationBuilderFunc = webApplicationBuilderFunc;
        }

        /// <summary>
        /// Builds the web application without running it.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task BuildAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_webApplicationBuilder == null)
            {
                throw new InvalidOperationException("WebApplicationBuilder not found.");
            }
            _webApplication = _webApplicationBuilder.Build();

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Used to initialize the web application builder used to build the actual web application.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public virtual async Task InitializeBuilderAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _webApplicationBuilder = await _webApplicationBuilderFunc();
        }

        ///<inheritdoc/>
        public override async Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
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
        public override async Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built");
            }

            _webApplication.Urls.Clear();
            _webApplication.Urls.Add(_url!);
            await _webApplication.StartAsync();
            _isRunning = true;
        }

        /// <summary>
        /// Stops the running web application
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override async Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_webApplication == null)
            {
                throw new InvalidOperationException("Web application not built and is not running.");
            }
            await _webApplication.StopAsync();
            await _webApplication.DisposeAsync();
            _isRunning = false;
        }
    }
}
