using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Represents a buildable runner, that executes additional methods before the main <see cref="IRunner.RunAsync"/> method.
    /// </summary>
    public interface IBuildableRunner : IRunner
    {
        /// <summary>
        /// Configures the required functionality in order to run the application instance decoupled from
        /// external dependencies such as external databases. 
        /// This method is async for compatibility reasons.
        /// </summary>
        /// <param name="scenarioSuiteContext">The scenario suite context.</param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -300)]
        Task ConfigureBuildAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <summary>
        /// Builds the runner without running it.
        /// </summary>
        /// <param name="scenarioSuiteContext">The scenario suite context.</param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -200)]
        Task BuildAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
