using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.Dummy
{
    /// <summary>
    /// Represents a dummy runner that doesn't really run anything but instead it suited for tests more like unit tests.
    /// </summary>
    public class DummyRunner : RunnerBase, IRunner
    {
        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _isRunning = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            _isRunning = false;
            return Task.CompletedTask;
        }
    }
}
