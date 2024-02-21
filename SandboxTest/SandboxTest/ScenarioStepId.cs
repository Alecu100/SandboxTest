namespace SandboxTest
{
    /// <summary>
    /// Represents the id of a scenario step used to identity a step for a spefic application instance and for interprocess communication.
    /// </summary>
    public class ScenarioStepId
    {
        /// <summary>
        /// Creates a new unnamed scenario step.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <param name="stepIndex"></param>
        public ScenarioStepId(string applicationInstanceId, int stepIndex)
        {
            ApplicationInstanceId = applicationInstanceId;
            StepIndex = stepIndex;
            Name = stepIndex.ToString();
        }

        /// <summary>
        /// Creates a new named scenarion step.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <param name="name"></param>
        public ScenarioStepId(string applicationInstanceId, string name)
        {
            ApplicationInstanceId = applicationInstanceId;
            Name = name;
        }

        /// <summary>
        /// The application instance id to which the step is assigned.
        /// </summary>
        public string ApplicationInstanceId { get; set; }

        /// <summary>
        /// The step index used to identity the step in case it doesn't have a name.
        /// </summary>
        public int StepIndex { get; set; }

        /// <summary>
        /// The step name used to identify the step to be more user friendly.
        /// </summary>
        public string Name { get; set; }
    }
}
