using SandboxTest.Scenario;

namespace SandboxTest.Internal
{
    /// <summary>
    /// For internal use only by the test runtime, should not be used externally
    /// </summary>
    public interface IScenarioStepRuntime
    {
        /// <summary>
        /// Called by the runtime to run the step using the given step context.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <returns></returns>
        Task RunAsync(IScenarioStepContext stepContext);
    }
}
