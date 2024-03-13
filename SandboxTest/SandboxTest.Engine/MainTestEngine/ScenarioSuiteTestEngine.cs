using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected List<ScenarioSuiteTestEngineApplicationInstance> _applicationInstances;
        protected IMainTestEngineRunContext? _mainTestEngineRunContext;
        protected Guid _runId;
        protected ScenarioRunResultType _allTestsFailedResultType;
        protected List<string> _allTestsFailedResultErrorMessages;
        protected Type? _scenarioSuiteType;
        protected object? _scenarioSuiteInstance;

        public ScenarioSuiteTestEngine()
        {
            _applicationInstances = new List<ScenarioSuiteTestEngineApplicationInstance>();
            _allTestsFailedResultErrorMessages = new List<string>();
        }

        public async virtual Task CloseApplicationInstancesAsync()
        {
            var stopApplicationInstancesTasks = new List<Task>();
            foreach (var applicationInstance in _applicationInstances)
            {
                stopApplicationInstancesTasks.Add(applicationInstance.StopInstanceAsync());
            }
            await Task.WhenAll(stopApplicationInstancesTasks);
            await Task.Delay(60000);
        }

        public virtual async Task LoadScenarioSuiteAsync(Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext, CancellationToken token)
        {
            if (_mainTestEngineRunContext != null || _applicationInstances.Any()) 
            {
                throw new InvalidOperationException("ScenarioSuiteTestEngine already has a scenario suite loaded for it.");
            }
            _runId = Guid.NewGuid();
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteType = scenarioSuiteType;

            if (scenarioSuiteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(constructor => constructor.GetParameters().Length > 0))
            {
                _allTestsFailedResultType = ScenarioRunResultType.Failed;
                _allTestsFailedResultErrorMessages.Add($"No constructor found for scenario suite type {scenarioSuiteType.Name} without arguments found");
                return;
            }
            if (token.IsCancellationRequested)
            {
                return;
            }
            var applicationInstancesMembers = GetApplicationInstancesMembers();
            try
            {
                _scenarioSuiteInstance = Activator.CreateInstance(scenarioSuiteType);
            }
            catch (Exception ex) 
            {
                _allTestsFailedResultType = ScenarioRunResultType.Failed;
                _allTestsFailedResultErrorMessages.Add($"Error creating scenario suite instance {ex}");
                return;
            }

            var startApplicationInstancesTasks = new List<Task>();
            foreach ( var applicationInstancesMember in applicationInstancesMembers) 
            {
                startApplicationInstancesTasks.Add(StartApplicationInstance(applicationInstancesMember));
            }
            await Task.WhenAll(startApplicationInstancesTasks);
        }

        public virtual async Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken token)
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            if (_allTestsFailedResultType == ScenarioRunResultType.Failed)
            {
                foreach (var scenarioMethod in scenarionMethods)
                {
                    var currentTime = DateTimeOffset.UtcNow;
                    await _mainTestEngineRunContext.OnScenarioRunningAsync(new Scenario(_scenarioSuiteType.Assembly, _scenarioSuiteType, scenarioMethod));
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly, 
                        _scenarioSuiteType, scenarioMethod, currentTime, TimeSpan.Zero, string.Join(Environment.NewLine, _allTestsFailedResultErrorMessages)));
                }
                return;
            }
        }

        protected virtual IEnumerable<FieldInfo> GetApplicationInstancesMembers()
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var applicationInstanceInterfaceType = typeof(IApplicationInstance);
            var applicationInstanceFields = new List<FieldInfo>();
            var allFields = new List<FieldInfo>();
            allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.Public));
            allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));

            foreach (var field in allFields)
            {
                if (applicationInstanceInterfaceType.IsAssignableFrom(field.FieldType))
                {
                    applicationInstanceFields.Add(field);
                }
            }

            return applicationInstanceFields;
        }

        protected virtual async Task StartApplicationInstance(FieldInfo applicationInstanceField)
        {
            if (_mainTestEngineRunContext == null || _scenarioSuiteType == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var applicationInstance = applicationInstanceField.GetValue(_scenarioSuiteInstance) as IApplicationInstance;
            if (applicationInstance == null)
            {
                _allTestsFailedResultType = ScenarioRunResultType.Failed;
                _allTestsFailedResultErrorMessages.Add($"Application instance for field {applicationInstanceField.Name} is missing");
                return;
            }

            var scenarioSuiteTestEngineApplicationInstance = new ScenarioSuiteTestEngineApplicationInstance(_runId, applicationInstance, _scenarioSuiteType, _mainTestEngineRunContext);
            await scenarioSuiteTestEngineApplicationInstance.StartInstanceAsync();
        }
    }
}
