namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// An operation that represents running a step for an application instance.
    /// </summary>
    public class RunStepOperation : Operation
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RunStepOperation"/> setting the proper type name of the operation
        /// </summary>
        public RunStepOperation(ScenarioStepId scenarioStepId)
        {
            TypeName = nameof(RunStepOperation);
            StepId = scenarioStepId;
        }

        /// <summary>
        /// The step id that identifies the step to run.
        /// </summary>
        public ScenarioStepId StepId { get; set; }
    }
}
