namespace SandboxTest.Scenario
{
    public interface IScenarioSuiteContext
    {
        /// <summary>
        /// Represents data shared accross an entire scenario suite. All data it contains must be json serializable.
        /// </summary>
        ScenarioSuiteData ScenarioSuiteData { get; }
    }
}
