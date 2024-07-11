using SandboxTest.Scenario;

namespace SandboxTest.Engine.Operations
{
    public class ScenarioSuiteOperationResult : OperationResult
    {
        public ScenarioSuiteData ScenarioSuiteData { get; set; }

        public ScenarioSuiteOperationResult(bool isSuccesful, ScenarioSuiteData scenarioSuiteData, string? errorMessage = null) : base(isSuccesful, errorMessage)
        {
            ScenarioSuiteData = scenarioSuiteData;
        }
    }
}
