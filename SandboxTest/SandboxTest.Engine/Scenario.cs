using System.Reflection;
using SandboxTest.Scenario;

namespace SandboxTest.Engine
{
    /// <summary>
    /// Class that represents a scenario with all its information like where it is found and what scenario suite type contains it.
    /// </summary>
    public class Scenario
    {
        private readonly string _scenarioSourceAssembly;
        private readonly string _scenarioMethodName;
        private readonly string _scenarioSuitTypeFullName;
        private readonly string? _description;
        private readonly string? _category;

        public Scenario(string scenarioSourceAssembly, string scenarioSuitTypeFullName, string scenarioMethodName, string? description = null, string? category = null)
        {
            _scenarioSourceAssembly = scenarioSourceAssembly;
            _scenarioMethodName = scenarioMethodName;
            _scenarioSuitTypeFullName = scenarioSuitTypeFullName;
            _description = description;
            _category = category;
        }

        public Scenario(Assembly sourceAssembly, Type scenarioSuiteTypeContainer, MethodInfo scenarioMethod)
        {
            _scenarioSourceAssembly = sourceAssembly.Location;
            _scenarioSuitTypeFullName = scenarioSuiteTypeContainer.FullName ?? throw new InvalidOperationException($"Scenario suit type with name {scenarioSuiteTypeContainer.Name} does not have a full name");
            _scenarioMethodName = scenarioMethod.Name;
        }

        public Scenario(Assembly sourceAssembly, Type scenarioSuiteTypeContainer, MethodInfo scenarioMethod, ScenarioSuiteAttribute scenarioSuiteAttribute, ScenarioAttribute scenarioAttribute)
            : this(sourceAssembly, scenarioSuiteTypeContainer, scenarioMethod)
        {
            _scenarioSourceAssembly = sourceAssembly.Location;
            _scenarioSuitTypeFullName = scenarioSuiteTypeContainer.FullName ?? throw new InvalidOperationException($"Scenario suit type with name {scenarioSuiteTypeContainer.Name} does not have a full name");
            _scenarioMethodName = scenarioMethod.Name;
            _category = scenarioSuiteAttribute.Name;
            _description = scenarioAttribute.Description;
        }

        /// <summary>
        /// The class that contains the scenarios.
        /// </summary>
        public string ScenarioSourceAssembly { get => _scenarioSourceAssembly; }

        /// <summary>
        /// The name of the class that contains the scenario suite.
        /// </summary>
        public string ScenarioSuitTypeFullName { get => _scenarioSuitTypeFullName; }

        /// <summary>
        /// The method containing the scenario that was ran.
        /// </summary>
        public string ScenarioMethodName { get => _scenarioMethodName; }

        /// <summary>
        /// Gets the description of the scenario.
        /// </summary>
        public string? Description { get => _description; }

        /// <summary>
        /// Gets the category of the scenario.
        /// </summary>
        public string? Category { get => _category; }
    }
}
