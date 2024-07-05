using System.Diagnostics;
using SandboxTest.Instance.Hosted;

namespace SandboxTest.Engine.MainTestEngine
{
    public interface IMainTestEngineRunContext : IHostedInstanceContext
    {
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
