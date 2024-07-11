using SandboxTest.Scenario;
namespace SandboxTest.Engine.ChildTestEngine
{
    public class ScenarioSuiteContext : IScenarioSuiteContext
    {
        private ScenarioSuiteData _scenarioSuiteData;

        public ScenarioSuiteContext(ScenarioSuiteData scenarioSuiteData) 
        {
            _scenarioSuiteData = scenarioSuiteData;
        }

        /// <inheritdoc/>
        public ScenarioSuiteData ScenarioSuiteData { get => _scenarioSuiteData; }
    }
}
