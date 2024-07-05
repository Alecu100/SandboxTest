namespace SandboxTest.Scenario
{
    /// <summary>
    /// Represents the id of a scenario step used to identity a step for a spefic application instance and for interprocess communication.
    /// </summary>
    public class ScenarioStepId
    {
        /// <summary>
        /// Only used for deserialization purposes
        /// </summary>
        public ScenarioStepId()
        {
            ApplicationInstanceId = string.Empty;
            StepIndex = 0;
        }

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
        /// The optional step name used to identify the step to be more user friendly.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Determines if the current step id is equal to the other step id passed as parameter
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not ScenarioStepId || obj == null)
            {
                return false;
            }

            var otherScenarioStepId = (ScenarioStepId)obj;
            return otherScenarioStepId.ApplicationInstanceId == ApplicationInstanceId && (otherScenarioStepId.Name == Name || StepIndex == otherScenarioStepId.StepIndex);
        }

        /// <summary>
        /// Returns a string representation of the current step id.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Name == null)
            {
                return $"Step {{ Application Instance: {ApplicationInstanceId}, Index: {StepIndex} }}";
            }

            return $"Step {{ Application Instance: {ApplicationInstanceId}, Name: {Name} }}";
        }

        /// <summary>
        /// Determines if two steps ids are equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(ScenarioStepId? first, ScenarioStepId? second)
        {
            if (first is null)
            {
                return second is not null;
            }
            if (second is null)
            {
                return first is null;
            }
            return first.Equals(second);
        }

        /// <summary>
        /// Determines if two steps ids are not equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(ScenarioStepId? first, ScenarioStepId? second)
        {
            if (first is null)
            {
                return second is not null;
            }
            if (second is null)
            {
                return first is not null;
            }
            return !first.Equals(second);
        }
    }
}
