using System.Reflection;

namespace SandboxTest.Engine
{
    /// <summary>
    /// Represents the results of running a scenarion
    /// </summary>
    public class ScenarioRunResult : OperationResult
    {
        private Type _scenarioContainer;
        private MethodInfo _scenarioMethod;
        private TimeSpan _duration;

        public ScenarioRunResult(bool isSuccesful, Type scenarioContainer, MethodInfo scenarioMethod, string? error, TimeSpan duration) : base(isSuccesful, error)
        {

            _scenarioContainer = scenarioContainer;
            _scenarioMethod = scenarioMethod;
            _duration = duration;
        }

        /// <summary>
        /// The test class which contains the method that represents the scenarion that was ran.
        /// </summary>
        public Type ScenarioContainer { get => _scenarioContainer; }

        /// <summary>
        /// Represents the method decorated with the scenarion attribute that contains the scenario.
        /// </summary>
        public MethodInfo ScenarioMethod { get => _scenarioMethod; }

        /// <summary>
        /// Denotes how long did the scenarion ran.
        /// </summary>
        public TimeSpan Duration { get => _duration; }
    }
}
