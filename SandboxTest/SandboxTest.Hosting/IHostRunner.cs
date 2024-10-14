using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Represents a runner that exposes a IHost and a IHostBuilder properties to be used by various application controllers.
    /// </summary>
    public interface IHostRunner : IBuildableRunner
    {
        /// <summary>
        /// Returns the host.
        /// </summary>
        IHost Host { get; }

        /// <summary>
        /// Returns the host builder.
        /// </summary>
        IHostBuilder HostBuilder { get; }

        /// <summary>
        /// Use the configure sandbox function to allow the host to run in a sandbox.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(BuildAsync), -300)]
        Task InitializeBuilderAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
