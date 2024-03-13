namespace SandboxTest.Engine.Operations
{
    public class RunScenarioStepOperationResult : OperationResult
    {
        /// <summary>
        /// Gets the newest step context resulted from running the last step.
        /// </summary>
        public ScenarioStepContext StepContext { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="RunScenarioStepOperationResult"/>
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="isSuccesful"></param>
        /// <param name="errorMessage"></param>
        public RunScenarioStepOperationResult(bool isSuccesful, ScenarioStepContext stepContext, string? errorMessage = default) : base(isSuccesful, errorMessage)
        {
            TypeName = nameof(RunScenarioStepOperationResult);
            StepContext = stepContext;
        }
    }
}
