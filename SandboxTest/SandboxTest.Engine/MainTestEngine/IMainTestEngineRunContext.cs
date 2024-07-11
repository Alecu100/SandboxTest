using System.Diagnostics;

namespace SandboxTest.Engine.MainTestEngine
{
    public interface IMainTestEngineRunContext
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

        /// <summary>
        /// Called when a scenarion has finished running.
        /// </summary>
        /// <param name="scenarioRunResult"></param>
        /// <returns></returns>
        Task OnScenarioRanAsync(ScenarioRunResult scenarioRunResult);

        /// <summary>
        /// Called when starting to run a scenario.
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        Task OnScenarioRunningAsync(Scenario scenario);

        /// <summary>
        /// Logs a message to be displayed in the test host runner.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task LogMessage(LogLevel logLevel, string message);
    }
}
