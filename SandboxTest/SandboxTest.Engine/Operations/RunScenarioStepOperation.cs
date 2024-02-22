namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// An operation that represents running a step for an application instance.
    /// </summary>
    public class RunScenarioStepOperation : Operation
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RunScenarioStepOperation"/> setting the proper type name of the operation
        /// </summary>
        public RunScenarioStepOperation(ScenarioStepId scenarioStepId)
        {
            TypeName = nameof(RunScenarioStepOperation);
            StepId = scenarioStepId;
        }

        /// <summary>
        /// The step id that identifies the step to run.
        /// </summary>
        public ScenarioStepId StepId { get; set; }
    }
}
