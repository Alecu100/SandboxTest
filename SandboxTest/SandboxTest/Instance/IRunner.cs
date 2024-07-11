using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Base interface for all runners exposing the standard functionality that they should implement.
    /// Not all methods ar required to actually do something such as ConfigureRunSandboxingAsync
    /// </summary>
    public interface IRunner : IAttachedMethodContainer
    {
        /// <summary>
        /// Runs the runner starting the instance it is assigned to.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        Task RunAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <summary>
        /// Stops the runner stopping the instance it is assigned to.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        Task StopAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <summary>
        /// Resets the current runner.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
