using SandboxTest.Scenario;

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
        public RunScenarioStepOperation(ScenarioStepId scenarioStepId, ScenarioStepData stepContext)
        {
            TypeName = nameof(RunScenarioStepOperation);
            StepId = scenarioStepId;
            StepContext = stepContext;
        }

        /// <summary>
        /// The step id that identifies the step to run.
        /// </summary>
        public ScenarioStepId StepId { get; set; }

        /// <summary>
        /// The step context used to pass data between steps. This
        /// </summary>
        public ScenarioStepData StepContext { get; set; }
    }
}
