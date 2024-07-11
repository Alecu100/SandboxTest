using SandboxTest.Scenario;

namespace SandboxTest.Engine.Operations
{
    public class RunInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="ResetInstanceOperation"/>
        /// </summary>
        public RunInstanceOperation(string applicationInstanceId, ScenarioSuiteData scenarioSuiteData)
        {
            TypeName = nameof(RunInstanceOperation);
            InstanceId = applicationInstanceId;
            ScenarioSuiteData = scenarioSuiteData;
        }

        /// <summary>
        /// The application instance to wait for for to be ready and loaded.
        /// </summary>
        public string InstanceId { get; private set; }

        /// <summary>
        /// The scenario suite data used to share data accross an entire scenario suite.
        /// </summary>
        public ScenarioSuiteData ScenarioSuiteData { get; set; }
    }
}
