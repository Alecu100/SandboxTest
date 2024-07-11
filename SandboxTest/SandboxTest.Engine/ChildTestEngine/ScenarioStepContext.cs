using SandboxTest.Scenario;

namespace SandboxTest.Engine.ChildTestEngine
{
    /// <summary>
    /// Represents the scenario step contex, giving access to the <see cref="ScenarioSuiteData"/> and <see cref="ScenarioSuiteData"/>.
    /// </summary>
    public class ScenarioStepContext : IScenarioStepContext
    {
        private ScenarioData _scenarioData;

        private ScenarioSuiteData _scenarioSuiteData;

        /// <inheritdoc/>
        public ScenarioData ScenarioData { get => _scenarioData; }

        /// <inheritdoc/>
        public ScenarioSuiteData ScenarioSuiteData {  get => _scenarioSuiteData; }

        /// <summary>
        /// Creates a new instance of the <see cref="ScenarioStepContext"/>.
        /// </summary>
        /// <param name="ScenarioSuiteData"></param>
        /// <param name="ScenarioData"></param>
        public ScenarioStepContext(ScenarioSuiteData scenarioSuiteData , ScenarioData scenarioData)
        {
            _scenarioSuiteData = scenarioSuiteData;
            _scenarioData = scenarioData;
        }
    }
}
