namespace SandboxTest.Engine
{
    public class ScenarioParameters
    {
        private readonly string _scenarioContainerFullyQualifiedName;
        private readonly string _scenarioMethodName;

        public ScenarioParameters(string scenarioContainer, string scenarionMethod)
        {
            _scenarioContainerFullyQualifiedName = scenarioContainer;
            _scenarioMethodName = scenarionMethod;
        }

        /// <summary>
        /// The class that contains the scenarios.
        /// </summary>
        public string ScenarioContainerFullyQualifiedName { get => _scenarioContainerFullyQualifiedName; }

        /// <summary>
        /// The method containing the scenario that was ran.
        /// </summary>
        public string ScenarioMethodName { get => _scenarioMethodName; }
    }
}
