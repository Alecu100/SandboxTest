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
        /// <param name="scenarioStepId">The step id that identifies the step to run.</param>
        /// <param name="scenarioSuiteData">The scenario data used to pass data between steps. This is serialized in json format and all the stored data must be json serializable.</param>
        /// <param name="scenarioData">The scenario suite data used to pass data between scenarios and the entire scenario suite. This is serialized in json format and all the stored data must be json serializable.</param>
        public RunScenarioStepOperation(ScenarioStepId scenarioStepId, ScenarioSuiteData scenarioSuiteData, ScenarioData scenarioData)
        {
            TypeName = nameof(RunScenarioStepOperation);
            StepId = scenarioStepId;
            ScenarioData = scenarioData;
            ScenarioSuiteData = scenarioSuiteData;
        }

        /// <summary>
        /// The step id that identifies the step to run.
        /// </summary>
        public ScenarioStepId StepId { get; set; }

        /// <summary>
        /// The scenario data used to pass data between steps. This is serialized in json format and all the stored data must be json serializable.
        /// </summary>
        public ScenarioData ScenarioData { get; set; }

        /// <summary>
        /// The scenario suite data used to pass data between scenarios and the entire scenario suite. This is serialized in json format and all the stored data must be json serializable.
        /// </summary>
        public ScenarioSuiteData ScenarioSuiteData { get; set; }
    }
}
