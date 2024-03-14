using SandboxTest.Engine.Operations;
using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected List<ScenarioSuiteTestEngineApplicationInstance> _scenarioSuiteApplicationInstances;
        protected IMainTestEngineRunContext? _mainTestEngineRunContext;
        protected Guid _runId;
        protected List<string> _scenarioFailedErrors;
        protected Type? _scenarioSuiteType;
        protected object? _scenarioSuiteInstance;
        protected Queue<List<(ScenarioSuiteTestEngineApplicationInstance, ScenarioStep)>> _stepsExecutionStages;
        protected List<ScenarioStepId> _allStepsIdsToExecute;

        public ScenarioSuiteTestEngine()
        {
            _scenarioSuiteApplicationInstances = new List<ScenarioSuiteTestEngineApplicationInstance>();
            _stepsExecutionStages = new Queue<List<(ScenarioSuiteTestEngineApplicationInstance, ScenarioStep)>>();
            _allStepsIdsToExecute = new List<ScenarioStepId>();
            _scenarioFailedErrors = new List<string>();
        }

        public async virtual Task CloseApplicationInstancesAsync()
        {
            var stopApplicationInstancesTasks = new List<Task>();
            foreach (var applicationInstance in _scenarioSuiteApplicationInstances)
            {
                stopApplicationInstancesTasks.Add(applicationInstance.StopInstanceAsync());
            }
            await Task.WhenAll(stopApplicationInstancesTasks);
            await Task.Delay(60000);
        }

        public virtual async Task LoadScenarioSuiteAsync(Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext, CancellationToken token)
        {
            if (_mainTestEngineRunContext != null || _scenarioSuiteApplicationInstances.Any()) 
            {
                throw new InvalidOperationException("ScenarioSuiteTestEngine already has a scenario suite loaded for it.");
            }
            _runId = Guid.NewGuid();
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteType = scenarioSuiteType;

            if (scenarioSuiteType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).All(constructor => constructor.GetParameters().Length > 0))
            {
                _scenarioFailedErrors.Add($"No constructor found for scenario suite type {scenarioSuiteType.Name} without arguments found");
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
 
                _scenarioFailedErrors.Add($"Error creating scenario suite instance {ex}");
                return;
            }

            var startApplicationInstancesTasks = new List<Task>();
            foreach ( var applicationInstancesMember in applicationInstancesMembers) 
            {
                startApplicationInstancesTasks.Add(StartApplicationInstanceAsync(applicationInstancesMember, token));
            }
            await Task.WhenAll(startApplicationInstancesTasks);
        }

        public virtual async Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken token)
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (_scenarioFailedErrors.Any())
            {
                foreach (var scenarioMethod in scenarionMethods)
                {
                    var currentTime = DateTimeOffset.UtcNow;
                    await _mainTestEngineRunContext.OnScenarioRunningAsync(new Scenario(_scenarioSuiteType.Assembly, _scenarioSuiteType, scenarioMethod));
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly, 
                        _scenarioSuiteType, scenarioMethod, currentTime, TimeSpan.Zero, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                }
                return;
            }

            foreach (var scenarioMethod in scenarionMethods)
            {
                await RunScenarioAsync(scenarioMethod, token);
            }
        }

        protected virtual async Task RunScenarioAsync(MethodInfo scenarioMethod, CancellationToken token)
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var startTime = DateTimeOffset.UtcNow;
            await _mainTestEngineRunContext.OnScenarioRunningAsync(new Scenario(_scenarioSuiteType.Assembly, _scenarioSuiteType, scenarioMethod));
            if (scenarioMethod.GetParameters().Length > 0)
            {
                await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                    _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, TimeSpan.Zero, $"Scenario method {scenarioMethod.Name} has parameters"));
                return;
            }

            try
            {
                var allAplicationInstancesTasks = new List<Task<OperationResult?>>();
                foreach (var applicationInstance in _scenarioSuiteApplicationInstances)
                {
                    allAplicationInstancesTasks.Add(applicationInstance.ResetInstanceAsync(token));
                }
                await Task.WhenAll(allAplicationInstancesTasks);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _scenarioFailedErrors.Clear();
                foreach (var aplicationInstanceTask in allAplicationInstancesTasks)
                {
                    var operationResult = await aplicationInstanceTask;
                    if (operationResult == null || operationResult.IsSuccesful == false)
                    {
                        _scenarioFailedErrors.Add($"Failed to reset application instance for scenario method {scenarioMethod.Name}");
                    }
                }
                if (_scenarioFailedErrors.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                        _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, startTime - DateTimeOffset.UtcNow, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                    return;
                }

                allAplicationInstancesTasks.Clear();
                foreach (var applicationInstance in _scenarioSuiteApplicationInstances)
                {
                    allAplicationInstancesTasks.Add(applicationInstance.LoadScenarioAsync(scenarioMethod.Name, token));
                }
                await Task.WhenAll(allAplicationInstancesTasks);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var aplicationInstanceTask in allAplicationInstancesTasks)
                {
                    var operationResult = await aplicationInstanceTask;
                    if (operationResult == null || operationResult.IsSuccesful == false)
                    {
                        _scenarioFailedErrors.Add($"Failed to load scenario method {scenarioMethod.Name} for application instance");
                    }
                }
                if (_scenarioFailedErrors.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                        _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, startTime - DateTimeOffset.UtcNow, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                    return;
                }

                var result = scenarioMethod.Invoke(_scenarioSuiteInstance, null);
                var resultTask = result as Task;
                if (resultTask != null)
                {
                    await resultTask;
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }

                _stepsExecutionStages.Clear();
                _allStepsIdsToExecute.Clear();

                var stillHasStepsToConfigure = false;
                do
                {
                    stillHasStepsToConfigure = false;
                    var currentStageSteps = new List<(ScenarioSuiteTestEngineApplicationInstance, ScenarioStep)>();

                    foreach (var scenarioSuiteApplicationInstance in _scenarioSuiteApplicationInstances)
                    {
                        var possibleStepsToExecute = scenarioSuiteApplicationInstance.Instance.Steps
                            .Where(step => !step.PreviousStepsIds.Any() || step.PreviousStepsIds.All(stepId => _allStepsIdsToExecute.Contains(stepId)))
                            .Select(step => (scenarioSuiteApplicationInstance, step));
                        if (possibleStepsToExecute.Any())
                        {
                            stillHasStepsToConfigure = true;
                            currentStageSteps.AddRange(possibleStepsToExecute);
                            _allStepsIdsToExecute.AddRange(possibleStepsToExecute.Select(step => step.step.Id));
                        }
                    }
                    if (stillHasStepsToConfigure)
                    {
                        _stepsExecutionStages.Enqueue(currentStageSteps);
                    }
                } while (stillHasStepsToConfigure);

                var allCyclicDependencySteps = new List<ScenarioStep>();
                foreach (var scenarioSuiteApplicationInstance in _scenarioSuiteApplicationInstances)
                {
                    var cyclicDependencySteps = scenarioSuiteApplicationInstance.Instance.Steps.Where(step => !_allStepsIdsToExecute.Contains(step.Id));
                    allCyclicDependencySteps.AddRange(cyclicDependencySteps);
                }
                if (allCyclicDependencySteps.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                      _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, startTime - DateTimeOffset.UtcNow, $"Detected cyclic dependencies between steps {string.Join(',', allCyclicDependencySteps)}"));
                    return;
                }

                var scenarioStepContext = new ScenarioStepContext();
                while (_stepsExecutionStages.Any())
                {
                    var currentStageApplicationSteps = _stepsExecutionStages.Dequeue();
                    var currentStageExecuteStepsTasks = new List<Task<OperationResult?>>();
                    foreach (var currentStageApplicationStep in currentStageApplicationSteps)
                    {
                        currentStageExecuteStepsTasks.Add(currentStageApplicationStep.Item1.ExecuteStepAsync(currentStageApplicationStep.Item2.Id, scenarioStepContext, token));
                    }
                    await Task.WhenAll(currentStageExecuteStepsTasks);

                    foreach (var runStepTask in currentStageExecuteStepsTasks)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        var runStepOperationResult = await runStepTask as RunScenarioStepOperationResult;
                        if (runStepOperationResult == null)
                        {
                            _scenarioFailedErrors.Add("Failed to run step with unknown error");
                            continue;
                        }
                        if (runStepOperationResult.IsSuccesful == false)
                        {
                            _scenarioFailedErrors.Add($"Failed to run step with error {runStepOperationResult.ErrorMessage}");
                            continue;
                        }
                        foreach (var scenarioStepContextKey in scenarioStepContext.Keys.ToArray())
                        {
                            if (!runStepOperationResult.StepContext.ContainsKey(scenarioStepContextKey))
                            {
                                scenarioStepContext.Remove(scenarioStepContextKey);
                            }
                        }
                        foreach (var scenarioStepContextItem in runStepOperationResult.StepContext)
                        {
                            scenarioStepContext.Add(scenarioStepContextItem.Key, scenarioStepContextItem.Value);
                        }
                    }

                    if (_scenarioFailedErrors.Any())
                    {
                        await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                            _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, startTime - DateTimeOffset.UtcNow, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                        return;
                    }
                }
                await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Successful, _scenarioSuiteType.Assembly,
                    _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, startTime - DateTimeOffset.UtcNow, $"Successfully ran scenario for method {scenarioMethod.Name}"));
        
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                    _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, TimeSpan.Zero, $"Error {ex} running the scenarion method {scenarioMethod.Name}"));
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

        protected virtual async Task StartApplicationInstanceAsync(FieldInfo applicationInstanceField, CancellationToken token)
        {
            if (_mainTestEngineRunContext == null || _scenarioSuiteType == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var applicationInstance = applicationInstanceField.GetValue(_scenarioSuiteInstance) as IApplicationInstance;
            if (applicationInstance == null)
            {
                _scenarioFailedErrors.Add($"Application instance for field {applicationInstanceField.Name} is missing");
                return;
            }

            var scenarioSuiteTestEngineApplicationInstance = new ScenarioSuiteTestEngineApplicationInstance(_runId, applicationInstance, _scenarioSuiteType, _mainTestEngineRunContext);
            await scenarioSuiteTestEngineApplicationInstance.StartInstanceAsync();
            var readyResult = await scenarioSuiteTestEngineApplicationInstance.ReadyInstanceAsync(token);
            if (readyResult == null || readyResult.IsSuccesful == false)
            {
                _scenarioFailedErrors.Add($"Failed to start application instanfce with id {applicationInstance.Id}");
            }
        }
    }
}
