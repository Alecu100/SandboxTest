using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.WebServer
{
    /// <summary>
    /// Represents a web server running externally from the current instance, mostly used when trying to access a web server
    /// that runs in a different process, container or machine than the current instance by manually specifying the url.
    /// </summary>
    public class RemoteWebServerRunner : RunnerBase, IWebServerRunner
    {
        private string? _url;
        private Func<string, Task>? _waitForServerToStartFunc;

        /// <summary>
        /// Gets and sets the set url for the remote web server.
        /// </summary>
        public string Url { get => _url ?? throw new InvalidOperationException("Url not set"); set => _url = value; }

        /// <summary>
        /// Configures the url of the remote web server.
        /// </summary>
        /// <param name="url"></param>
        public void OnConfigureUrl(string url)
        {
            _url = url;
        }

        /// <summary>
        /// Provides a way to wait for the remote web server to start before finishing running the remote web server controller.
        /// </summary>
        /// <param name="resetFunc"></param>
        public void OnConfigureWaitForServerToStart(Func<string, Task> waitForServerToStartFunc)
        {
            _waitForServerToStartFunc = waitForServerToStartFunc;
        }

        /// <summary>
        /// Remote web server does not actually have any reset functionality.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            return Task.CompletedTask;
        }

        public override async Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _ = Url;

            if (_waitForServerToStartFunc != null)
            {
                await _waitForServerToStartFunc(Url);
            }
        }

        /// <summary>
        /// Remote web server does not actually have any stop functionality.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            return Task.CompletedTask;
        }
    }
}
