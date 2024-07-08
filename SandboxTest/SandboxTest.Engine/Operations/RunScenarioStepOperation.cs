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
            StepData = stepContext;
        }

        /// <summary>
        /// The step id that identifies the step to run.
        /// </summary>
        public ScenarioStepId StepId { get; set; }

        /// <summary>
        /// The step data used to pass data between steps. This is serialized in json format and all the stored data must be json serializable.
        /// </summary>
        public ScenarioStepData StepData { get; set; }
    }
}
