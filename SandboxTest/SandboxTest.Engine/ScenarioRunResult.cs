using System.Reflection;

namespace SandboxTest.Engine
{
    /// <summary>
    /// Represents the results of running a scenario.
    /// </summary>
    public class ScenarioRunResult
    {
        private readonly TimeSpan _duration;
        private readonly Scenario _scenarioInfo;
        private readonly string? _errorMessage;
        private readonly Exception? _errorException;
        private readonly ScenarioRunResultType _result;
        private readonly DateTimeOffset _startTime;

        public ScenarioRunResult(ScenarioRunResultType result, Assembly sourceAssembly, Type scenarioSuiteTypeContainer, MethodInfo scenarioMethod, DateTimeOffset startTime, TimeSpan duration, 
            string? errorMessage = null, Exception? errorException = null)
        {
            _result = result;
            _startTime = startTime;
            _duration = duration;
            _scenarioInfo = new Scenario(sourceAssembly, scenarioSuiteTypeContainer, scenarioMethod);
            _errorMessage = errorMessage;
            _errorException = errorException;
        }

        public ScenarioRunResult(ScenarioRunResultType result, string scenarioSourceAssembly, string scenarioSuitTypeFullName, string scenarioMethodName, DateTimeOffset startTime, TimeSpan duration,
            string? errorMessage = null, Exception? errorException = null)
        {
            _result = result;
            _startTime = startTime;
            _duration = duration;
            _scenarioInfo = new Scenario(scenarioSourceAssembly, scenarioSuitTypeFullName, scenarioMethodName);
            _errorMessage = errorMessage;
            _errorException = errorException;
        }

        public ScenarioRunResult(ScenarioRunResultType result, Scenario scenario, DateTimeOffset startTime, TimeSpan duration,
            string? errorMessage = null, Exception? errorException = null)
        {
            _result = result;
            _startTime = startTime;
            _duration = duration;
            _scenarioInfo = scenario;
            _errorMessage = errorMessage;
            _errorException = errorException;
        }

        /// <summary>
        /// Gets the startime of the test.
        /// </summary>
        public DateTimeOffset StartTime { get => _startTime; }

        /// <summary>
        /// Gets the result type of the run, like for example if it ran succesfully or not.
        /// </summary>
        public ScenarioRunResultType Result {  get => _result; }

        /// <summary>
        /// Gets the scenario that was ran.
        /// </summary>
        public Scenario Scenario { get => _scenarioInfo; }

        /// <summary>
        /// Gets the error raised while running the scenario.
        /// </summary>
        public string? ErrorMessage { get => _errorMessage; }

        /// <summary>
        /// Gets the exception raised while executing the scenario
        /// </summary>
        public Exception? ErrorException { get => _errorException; }

        /// <summary>
        /// Denotes how long did the scenarion ran.
        /// </summary>
        public TimeSpan Duration { get => _duration; }
    }
}
