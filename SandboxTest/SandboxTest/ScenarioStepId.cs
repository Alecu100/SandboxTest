namespace SandboxTest
{
    /// <summary>
    /// Represents the id of a scenario step used to identity a step for a spefic application instance and for interprocess communication.
    /// </summary>
    public class ScenarioStepId
    {
        private readonly string _applicationInstanceId;
        private readonly int _stepIndex;

        public ScenarioStepId(string applicationInstanceId, int stepIndex)
        {
            _applicationInstanceId = applicationInstanceId;
            _stepIndex = stepIndex;
        }

        public string ApplicationInstanceId { get; }

        public int StepIndex { get; }
    }
}
