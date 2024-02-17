namespace SandboxTest.Engine
{
    public class ScenarioParameters
    {
        private readonly string _scenarioContainerFullyQualifiedName;
        private readonly string _scenarioMethodName;
        private readonly string? _description;
        private readonly string? _category;

        public ScenarioParameters(string scenarioContainer, string scenarionMethod, string? description, string? category)
        {
            _scenarioContainerFullyQualifiedName = scenarioContainer;
            _scenarioMethodName = scenarionMethod;
            _description = description;
            _category = category;
        }

        /// <summary>
        /// The class that contains the scenarios.
        /// </summary>
        public string ScenarioContainerFullyQualifiedName { get => _scenarioContainerFullyQualifiedName; }

        /// <summary>
        /// The method containing the scenario that was ran.
        /// </summary>
        public string ScenarioMethodName { get => _scenarioMethodName; }

        /// <summary>
        /// Gets the description of the scenario
        /// </summary>
        public string? Description { get => _description; }

        /// <summary>
        /// Gets the category of the scenario
        /// </summary>
        public string? Category { get => _category; }
    }
}
