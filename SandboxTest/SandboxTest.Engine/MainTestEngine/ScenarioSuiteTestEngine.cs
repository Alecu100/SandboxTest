using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected List<ScenarioSuiteTestEngineApplicationInstance> _applicationInstances;
        protected IMainTestEngineRunContext? _mainTestEngineRunContext;
        protected Guid _runId;
        protected ScenarioRunResultType _allTestsFailedResultType;
        protected string? _allTestsFailedResultErrorMessage;
        protected Type? _scenarionSuiteType;

        public ScenarioSuiteTestEngine()
        {
            _applicationInstances = new List<ScenarioSuiteTestEngineApplicationInstance>();
        }

        public virtual Task CloseApplicationInstancesAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task LoadScenarioSuiteAsync(Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext, CancellationToken token)
        {
            if (_mainTestEngineRunContext != null || _applicationInstances.Any()) 
            {
                throw new InvalidOperationException("ScenarioSuiteTestEngine already has a scenario suite loaded for it.");
            }
            _runId = Guid.NewGuid();
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarionSuiteType = scenarioSuiteType;

            if (scenarioSuiteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(constructor => constructor.GetParameters().Length > 0))
            {
                _allTestsFailedResultType = ScenarioRunResultType.Failed;
                _allTestsFailedResultErrorMessage = $"No constructor found for scenario suite type {scenarioSuiteType.Name} without arguments found";
                return;
            }

            var declaredApplicationInstancesFields = scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var declaredApplicationInstancesProperties = scenarioSuiteType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        public virtual async Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken token)
        {
            if (_scenarionSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            if (_allTestsFailedResultType == ScenarioRunResultType.Failed)
            {
                foreach (var scenarioMethod in scenarionMethods)
                {
                    var currentTime = DateTimeOffset.UtcNow;
                    await _mainTestEngineRunContext.OnScenarioRunningAsync(new Scenario(_scenarionSuiteType.Assembly, _scenarionSuiteType, scenarioMethod));
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarionSuiteType.Assembly, 
                        _scenarionSuiteType, scenarioMethod, currentTime, TimeSpan.Zero, _allTestsFailedResultErrorMessage));
                }
                return;
            }
        }
    }
}
