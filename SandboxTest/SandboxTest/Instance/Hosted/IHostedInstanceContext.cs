using SandboxTest.Scenario;
using System.Diagnostics;

namespace SandboxTest.Instance.Hosted
{
    /// <summary>
    /// Represents the context in which a hosted instances runs enabling it to start another process.
    /// </summary>
    public interface IHostedInstanceContext : IScenarioSuiteContext
    {
        /// <summary>
        /// Whether the scenario is being debugged or not.
        /// </summary>
        bool IsBeingDebugged { get; }

        /// <summary>
        /// Launches a new process.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="debug"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="arguments"></param>
        /// <param name="environmentVariables"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Process> LaunchProcessAsync(string filePath, bool debug, string? workingDirectory = null, string? arguments = null, IDictionary<string, string?>? environmentVariables = null, CancellationToken token = default);
    }
}
