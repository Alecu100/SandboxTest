using SandboxTest.Engine.ChildTestEngine;
using SandboxTest.Engine.Operations;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected IScenarioSuiteInitializer _scenarioSuiteInitializer;
        protected List<ScenarioSuiteTestEngineInstanceHandler> _scenarioSuiteInstances;
        protected IMainTestEngineRunContext? _mainTestEngineRunContext;
        protected Guid _runId;
        protected List<string> _scenarioFailedErrors;
        protected Type? _scenarioSuiteType;
        protected object? _scenarioSuiteInstance;
        protected Queue<ScenarioSuiteStepsStage> _stepsExecutionStages;
        protected HashSet<ScenarioStepId> _allStepsIdsToExecute;
        protected ScenarioSuiteData? _scenarioSuiteData;
        protected readonly ScenariosAssemblyLoadContext _scenariosAssemblyLoadContext;

        public ScenarioSuiteTestEngine(ScenariosAssemblyLoadContext scenariosAssemblyLoadContext)
        {
            _scenarioSuiteInitializer = new ScenarioSuiteInitializer();
            _scenariosAssemblyLoadContext = scenariosAssemblyLoadContext;
            _scenarioSuiteInstances = new List<ScenarioSuiteTestEngineInstanceHandler>();
            _stepsExecutionStages = new Queue<ScenarioSuiteStepsStage>();
            _allStepsIdsToExecute = new HashSet<ScenarioStepId>();
            _scenarioFailedErrors = new List<string>();
        }

        public async virtual Task CloseInstancesAsync()
        {
            var stopInstancesTasks = new List<Task<OperationResult>>();
            var scenarioSuiteInstancesWithoutOrder = _scenarioSuiteInstances.Where(scenarioSuiteInstance => scenarioSuiteInstance.Instance.Order == null);

            foreach (var scenarioSuiteInstanceWithoutOrder in scenarioSuiteInstancesWithoutOrder)
            {
                stopInstancesTasks.Add(scenarioSuiteInstanceWithoutOrder.StopInstanceAsync(_scenarioSuiteData!));
            }
            await Task.WhenAll(stopInstancesTasks);
            foreach (var stopInstanceTask  in stopInstancesTasks)
            {
                var scenarioSuiteOperationResult = (await stopInstanceTask) as ScenarioSuiteOperationResult;
                if (scenarioSuiteOperationResult?.ScenarioSuiteData != null)
                {
                    MergeDataDictionaries(_scenarioSuiteData!, scenarioSuiteOperationResult.ScenarioSuiteData!);
                }
            }

            var scenarioSuiteInstancesWithOrderGroups = _scenarioSuiteInstances.Where(scenarioSuiteApplicationInstance => scenarioSuiteApplicationInstance.Instance.Order != null)
                .GroupBy(scenarioSuiteInstance => scenarioSuiteInstance.Instance.Order).OrderByDescending(x => x.Key);
            foreach (var scenarioSuiteInstanceWithOrderGroup in scenarioSuiteInstancesWithOrderGroups)
            {
                stopInstancesTasks.Clear();
                foreach (var scenarioSuiteInstanceWithOrder in scenarioSuiteInstanceWithOrderGroup)
                {
                    stopInstancesTasks.Add(scenarioSuiteInstanceWithOrder.StopInstanceAsync(_scenarioSuiteData!));
                }
                await Task.WhenAll(stopInstancesTasks);
                foreach (var stopApplicationInstanceTask in stopInstancesTasks)
                {
                    var scenarioSuiteOperationResult = (await stopApplicationInstanceTask) as ScenarioSuiteOperationResult;
                    if (scenarioSuiteOperationResult?.ScenarioSuiteData != null)
                    {
                        MergeDataDictionaries(_scenarioSuiteData!, scenarioSuiteOperationResult.ScenarioSuiteData!);
                    }
                }
            }
        }

        public virtual async Task LoadScenarioSuiteAsync(Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext, CancellationToken token)
        {
            if (_mainTestEngineRunContext != null || _scenarioSuiteInstances.Any()) 
            {
                throw new InvalidOperationException("ScenarioSuiteTestEngine already has a scenario suite loaded for it.");
            }
            _scenarioSuiteData = new ScenarioSuiteData();
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
            var instancesMembers = GetInstancesMembers();
            try
            {
                _scenarioSuiteInstance = Activator.CreateInstance(scenarioSuiteType);
                if (_scenarioSuiteInstance == null)
                {
                    _scenarioFailedErrors.Add("Error creating scenario suite instance");
                    return;
                }
                _scenarioSuiteInitializer.Initialize(_scenarioSuiteInstance);
            }
            catch (Exception ex) 
            {
                 _scenarioFailedErrors.Add($"Error creating scenario suite instance {ex}");
                return;
            }

            await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, "Starting instances");
            var instancesToStart = new List<IInstance>();
            foreach (var instanceMember in instancesMembers ) 
            {
                var instance = instanceMember.GetValue(_scenarioSuiteInstance) as IInstance;
                if (instance == null)
                {
                    _scenarioFailedErrors.Add($"Instance for field {instanceMember.Name} is missing");
                    continue;
                }
                instancesToStart.Add(instance);
            }
            var orderedInstances = instancesToStart.GroupBy(instance => instance.Order).OrderBy(instances => instances.Key);

            foreach (var instancesMember in orderedInstances) 
            {
                var startInstancesTasks = new List<Task<OperationResult?>>();
                foreach (var instance in instancesMember)
                {
                    startInstancesTasks.Add(StartInstanceAsync(instance, token));
                }
                await Task.WhenAll(startInstancesTasks);

                foreach (var startInstancesResultTask in startInstancesTasks)
                {
                    var scenarioSuiteOperationResult = (await startInstancesResultTask) as ScenarioSuiteOperationResult;
                    if (scenarioSuiteOperationResult == null) 
                    {
                        _scenarioFailedErrors.Add($"Failed to start instance");
                    }
                }
            }
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

            await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Running application scenario method {scenarioMethod.Name}");

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
                var allInstancesTasks = new List<Task<OperationResult?>>();
                foreach (var applicationInstance in _scenarioSuiteInstances)
                {
                    allInstancesTasks.Add(applicationInstance.ResetInstanceAsync(_scenarioSuiteData!, token));
                }
                await Task.WhenAll(allInstancesTasks);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _scenarioFailedErrors.Clear();
                foreach (var aplicationInstanceTask in allInstancesTasks)
                {
                    var operationResult = (ScenarioSuiteOperationResult?)(await aplicationInstanceTask);
                    if (operationResult == null || operationResult.IsSuccesful == false)
                    {
                        _scenarioFailedErrors.Add($"Failed to reset application instance for scenario method {scenarioMethod.Name}");
                        continue;
                    }
                    MergeDataDictionaries(_scenarioSuiteData!, operationResult.ScenarioSuiteData!);
                }
                if (_scenarioFailedErrors.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                        _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime, string.Join(Environment.NewLine, _scenarioFailedErrors)));
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

                allInstancesTasks.Clear();
                foreach (var applicationInstance in _scenarioSuiteInstances)
                {
                    allInstancesTasks.Add(applicationInstance.LoadScenarioAsync(scenarioMethod.Name, token));
                }
                await Task.WhenAll(allInstancesTasks);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var instanceTask in allInstancesTasks)
                {
                    var operationResult = await instanceTask;
                    if (operationResult == null || operationResult.IsSuccesful == false)
                    {
                        _scenarioFailedErrors.Add($"Failed to load scenario method {scenarioMethod.Name} for instance");
                    }
                }
                if (_scenarioFailedErrors.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                        _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                    return;
                }

                _stepsExecutionStages.Clear();
                _allStepsIdsToExecute.Clear();

                var stillHasStepsToConfigure = false;
                do
                {
                    stillHasStepsToConfigure = false;
                    var currentScenarioSuiteStepsStage = new ScenarioSuiteStepsStage();

                    foreach (var scenarioSuiteApplicationInstance in _scenarioSuiteInstances)
                    {
                        foreach (var step in scenarioSuiteApplicationInstance.Instance.Steps)
                        {
                            if (!_allStepsIdsToExecute.Contains(step.Id) && (!step.PreviousStepsIds.Any() || step.PreviousStepsIds.All(stepId => _allStepsIdsToExecute.Contains(stepId))))
                            {
                                currentScenarioSuiteStepsStage.AddInstanceStep(scenarioSuiteApplicationInstance, step);
                                stillHasStepsToConfigure = true;
                            }
                        }
                    }
                    if (stillHasStepsToConfigure)
                    {
                        foreach (var stageStep in currentScenarioSuiteStepsStage.AllInstanceSteps)
                        {
                            _allStepsIdsToExecute.Add(stageStep.Id);
                        }
                        _stepsExecutionStages.Enqueue(currentScenarioSuiteStepsStage);
                    }
                } while (stillHasStepsToConfigure);

                var allCyclicDependencySteps = new List<ScenarioStep>();
                foreach (var scenarioSuiteInstance in _scenarioSuiteInstances)
                {
                    var cyclicDependencySteps = scenarioSuiteInstance.Instance.Steps.Where(step => !_allStepsIdsToExecute.Contains(step.Id));
                    allCyclicDependencySteps.AddRange(cyclicDependencySteps);
                }
                if (allCyclicDependencySteps.Any())
                {
                    await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                      _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime, $"Detected cyclic dependencies between steps {string.Join(',', allCyclicDependencySteps)}"));
                    return;
                }

                var scenarioData = new ScenarioData();
                while (_stepsExecutionStages.Any())
                {
                    var currentStageInstanceSteps = _stepsExecutionStages.Dequeue();
                    await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Running step execution stage with steps {currentStageInstanceSteps}");
                    var currentStageExecuteStepsTasks = new List<Task<List<RunScenarioStepOperationResult?>>>();
                    foreach (var currentStageInstanceStep in currentStageInstanceSteps.InstanceHandlersSteps)
                    {
                        currentStageExecuteStepsTasks.Add(RunStepsForInstanceAsync(currentStageInstanceStep, scenarioData, token));
                    }
                    await Task.WhenAll(currentStageExecuteStepsTasks);
                    await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Finished running step execution stage with steps {currentStageInstanceSteps}");

                    foreach (var runStepsTask in currentStageExecuteStepsTasks)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        var runStepsOperationResults = await runStepsTask;
                        if (runStepsOperationResults == null)
                        {
                            _scenarioFailedErrors.Add("Failed to run step with unknown error");
                            continue;
                        }
                        foreach (var runStepOperationResult in runStepsOperationResults)
                        {
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
                            MergeDataDictionaries(scenarioData, runStepOperationResult.ScenarioData);
                        }
                    }

                    if (_scenarioFailedErrors.Any())
                    {
                        await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Failed, _scenarioSuiteType.Assembly,
                            _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime, string.Join(Environment.NewLine, _scenarioFailedErrors)));
                        return;
                    }
                }
                await _mainTestEngineRunContext.OnScenarioRanAsync(new ScenarioRunResult(ScenarioRunResultType.Successful, _scenarioSuiteType.Assembly,
                    _scenarioSuiteType, scenarioMethod, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - startTime, $"Successfully ran scenario for method {scenarioMethod.Name}"));
        
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

        private void MergeDataDictionaries(Dictionary<string, object?> target, Dictionary<string, object?> changes)
        {
            if (ReferenceEquals(target, changes))
            {
                return;
            }
            foreach (var changedKeys in target.Keys.ToArray())
            {
                if (!changes.ContainsKey(changedKeys))
                {
                    target.Remove(changedKeys);
                }
            }
            foreach (var changesPair in changes)
            {
                target[changesPair.Key] = changesPair.Value;
            }
        }

        private async Task<List<RunScenarioStepOperationResult?>> RunStepsForInstanceAsync(KeyValuePair<ScenarioSuiteTestEngineInstanceHandler, List<ScenarioStep>> applicationInstanceSteps, ScenarioData scenarioData, CancellationToken token)
        {
            var allStepsResults = new List<RunScenarioStepOperationResult?>();
            foreach (var instanceStep in applicationInstanceSteps.Value)
            {
                var result = await applicationInstanceSteps.Key.ExecuteStepAsync(instanceStep, _scenarioSuiteData!, scenarioData, token);
                allStepsResults.Add(result as RunScenarioStepOperationResult);
            }
            return allStepsResults;
        }

        protected virtual IEnumerable<FieldInfo> GetInstancesMembers()
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var applicationInstanceInterfaceType = typeof(IInstance);
            var instanceFields = new List<FieldInfo>();
            var allFields = new List<FieldInfo>();
            allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.Public));
            allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));

            foreach (var field in allFields)
            {
                if (applicationInstanceInterfaceType.IsAssignableFrom(field.FieldType))
                {
                    instanceFields.Add(field);
                }
            }

            return instanceFields;
        }

        protected virtual async Task<OperationResult?> StartInstanceAsync(IInstance instance, CancellationToken token)
        {
            if (_mainTestEngineRunContext == null || _scenarioSuiteType == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var scenarioSuiteTestEngineApplicationInstance = new ScenarioSuiteTestEngineInstanceHandler(_runId, instance, _scenariosAssemblyLoadContext, _scenarioSuiteType, _mainTestEngineRunContext);
            ScenarioSuiteData? scenarioSuiteData;
            try
            {
                scenarioSuiteData = await scenarioSuiteTestEngineApplicationInstance.LoadInstanceAsync(_scenarioSuiteData!, token);
            }
            catch(Exception ex)
            {
                _scenarioFailedErrors.Add($"Instance {instance.Id} failed to start with exception {ex}");
                return null;
            }
            await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Running runner in instance {instance.Id}");
            _scenarioSuiteInstances.Add(scenarioSuiteTestEngineApplicationInstance);
            if (instance.Order != null)
            {
                var runInstanceResult = await scenarioSuiteTestEngineApplicationInstance.RunInstanceAsync(_scenarioSuiteData!, token) as ScenarioSuiteOperationResult;
                if (runInstanceResult == null || runInstanceResult.IsSuccesful == false)
                {
                    _scenarioFailedErrors.Add($"Failed to start instance with id {instance.Id} with error: {runInstanceResult?.ErrorMessage}");
                }
                await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Instance {instance.Id} started succesfully");
                if (scenarioSuiteData != null && runInstanceResult?.ScenarioSuiteData != null)
                {
                    MergeDataDictionaries(runInstanceResult.ScenarioSuiteData!, scenarioSuiteData!);
                }
                return runInstanceResult;
            }
            return new ScenarioSuiteOperationResult(true, _scenarioSuiteData!);
        }
    }
}
