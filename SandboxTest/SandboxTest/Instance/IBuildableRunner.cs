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
        /// Builds the runner without running it.
        /// </summary>
        /// <param name="scenarioSuiteContext">The scenario suite context.</param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -200)]
        Task BuildAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
