using SandboxTest.Scenario;

namespace SandboxTest.Engine.MainTestEngine
{
    /// <summary>
    /// Represents a scenario suite step stage with associated steps for each application instance handler to run.
    /// </summary>
    public class ScenarioSuiteStepsStage
    {
        private Dictionary<ScenarioSuiteTestEngineInstanceHandler, List<ScenarioStep>> _instanceHandlersSteps;
        private List<ScenarioStep> _allInstanceSteps;

        public ScenarioSuiteStepsStage()
        {
            _instanceHandlersSteps = new Dictionary<ScenarioSuiteTestEngineInstanceHandler, List<ScenarioStep>>();
            _allInstanceSteps = new List<ScenarioStep>();
        }

        public void AddInstanceStep(ScenarioSuiteTestEngineInstanceHandler instanceHandler, ScenarioStep scenarioStep)
        {
            if (!_instanceHandlersSteps.ContainsKey(instanceHandler))
            {
                _instanceHandlersSteps[instanceHandler] = new List<ScenarioStep>();
            }
            _instanceHandlersSteps[instanceHandler].Add(scenarioStep);
            _allInstanceSteps.Add(scenarioStep);
        }

        public IReadOnlyDictionary<ScenarioSuiteTestEngineInstanceHandler, List<ScenarioStep>> InstanceHandlersSteps { get => _instanceHandlersSteps; }

        public IReadOnlyList<ScenarioStep> AllInstanceSteps {  get => _allInstanceSteps; }

        public override string ToString()
        {
            return $"Scenario Run State {string.Join(',', _allInstanceSteps.Select(x => x.Id))}";
        }
    }
}
