namespace SandboxTest.Scenario
{
    public interface IScenarioStepContext : IScenarioSuiteContext
    {
        /// <summary>
        /// Represents data shared accross an scenario. All data it contains must be json serializable.
        /// </summary>
        ScenarioData ScenarioData { get; }
    }
}
