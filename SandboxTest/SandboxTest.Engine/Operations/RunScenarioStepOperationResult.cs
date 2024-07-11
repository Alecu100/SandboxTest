using SandboxTest.Scenario;

namespace SandboxTest.Engine.Operations
{
    public class RunScenarioStepOperationResult : OperationResult
    {
        /// <summary>
        /// Gets the changed scenario data resulted from running the step.
        /// </summary>
        public ScenarioData ScenarioData { get; set; }

        /// <summary>
        /// Gets the changed scenario suite data resulted from running the step.
        /// </summary>
        public ScenarioSuiteData ScenarioSuiteData { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="RunScenarioStepOperationResult"/>
        /// </summary>
        /// <param name="scenarioData"></param>
        /// <param name="isSuccesful"></param>
        /// <param name="errorMessage"></param>
        public RunScenarioStepOperationResult(bool isSuccesful, ScenarioSuiteData scenarioSuiteData, ScenarioData scenarioData, string? errorMessage = default) : base(isSuccesful, errorMessage)
        {
            TypeName = nameof(RunScenarioStepOperationResult);
            ScenarioData = scenarioData;
            ScenarioSuiteData = scenarioSuiteData;
        }
    }
}
