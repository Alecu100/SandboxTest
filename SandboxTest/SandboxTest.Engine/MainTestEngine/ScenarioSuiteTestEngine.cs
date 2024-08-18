using SandboxTest.Engine.Operations;
using SandboxTest.Instance;
using SandboxTest.Loader;
using SandboxTest.Scenario;
using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected List<ScenarioSuiteTestEngineInstanceHandler> _scenarioSuiteApplicationInstances;
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
            _scenariosAssemblyLoadContext = scenariosAssemblyLoadContext;
            _scenarioSuiteApplicationInstances = new List<ScenarioSuiteTestEngineInstanceHandler>();
            _stepsExecutionStages = new Queue<ScenarioSuiteStepsStage>();
            _allStepsIdsToExecute = new HashSet<ScenarioStepId>();
            _scenarioFailedErrors = new List<string>();
        }

        public async virtual Task CloseApplicationInstancesAsync()
        {
            var stopApplicationInstancesTasks = new List<Task<OperationResult>>();
            var scenarioSuiteApplicationInstancesWithoutOrder = _scenarioSuiteApplicationInstances.Where(scenarioSuiteApplicationInstance => scenarioSuiteApplicationInstance.Instance.Order == null);

            foreach (var scenarioSuiteApplicationInstanceWithoutOrder in scenarioSuiteApplicationInstancesWithoutOrder)
            {
                stopApplicationInstancesTasks.Add(scenarioSuiteApplicationInstanceWithoutOrder.StopInstanceAsync(_scenarioSuiteData!));
            }
            await Task.WhenAll(stopApplicationInstancesTasks);
            foreach (var stopApplicationInstanceTask  in stopApplicationInstancesTasks)
            {
                var scenarioSuiteOperationResult = (await stopApplicationInstanceTask) as ScenarioSuiteOperationResult;
                if (scenarioSuiteOperationResult?.ScenarioSuiteData != null)
                {
                    MergeDataDictionaries(_scenarioSuiteData!, scenarioSuiteOperationResult.ScenarioSuiteData!);
                }
            }

            var scenarioSuiteApplicationInstancesWithOrderGroups = _scenarioSuiteApplicationInstances.Where(scenarioSuiteApplicationInstance => scenarioSuiteApplicationInstance.Instance.Order != null)
                .GroupBy(scenarioSuiteApplicationInstance => scenarioSuiteApplicationInstance.Instance.Order).OrderByDescending(x => x.Key);
            foreach (var scenarioSuiteApplicationInstanceWithOrderGroup in scenarioSuiteApplicationInstancesWithOrderGroups)
            {
                stopApplicationInstancesTasks.Clear();
                foreach (var scenarioSuiteApplicationInstanceWithOrder in scenarioSuiteApplicationInstanceWithOrderGroup)
                {
                    stopApplicationInstancesTasks.Add(scenarioSuiteApplicationInstanceWithOrder.StopInstanceAsync(_scenarioSuiteData!));
                }
                await Task.WhenAll(stopApplicationInstancesTasks);
                foreach (var stopApplicationInstanceTask in stopApplicationInstancesTasks)
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
            if (_mainTestEngineRunContext != null || _scenarioSuiteApplicationInstances.Any()) 
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

            await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, "Starting application instances");
            var instancesToStart = new List<IInstance>();
            foreach (var applicationInstanceMember in applicationInstancesMembers ) 
            {
                var instance = applicationInstanceMember.GetValue(_scenarioSuiteInstance) as IInstance;
                if (instance == null)
                {
                    _scenarioFailedErrors.Add($"Application instance for field {applicationInstanceMember.Name} is missing");
                    continue;
                }
                instancesToStart.Add(instance);
            }
            var orderedInstances = instancesToStart.GroupBy(instance => instance.Order).OrderBy(instances => instances.Key);

            foreach (var applicationInstancesMember in orderedInstances) 
            {
                var startApplicationInstancesTasks = new List<Task<OperationResult?>>();
                foreach (var instance in applicationInstancesMember)
                {
                    startApplicationInstancesTasks.Add(StartApplicationInstanceAsync(instance, token));
                }
                await Task.WhenAll(startApplicationInstancesTasks);

                foreach (var startApplicationInstancesResultTask in startApplicationInstancesTasks)
                {
                    var scenarioSuiteOperationResult = (await startApplicationInstancesResultTask) as ScenarioSuiteOperationResult;
                    if (scenarioSuiteOperationResult == null) 
                    {
                        _scenarioFailedErrors.Add($"Failed to start application instance");
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
                var allAplicationInstancesTasks = new List<Task<OperationResult?>>();
                foreach (var applicationInstance in _scenarioSuiteApplicationInstances)
                {
                    allAplicationInstancesTasks.Add(applicationInstance.ResetInstanceAsync(_scenarioSuiteData!, token));
                }
                await Task.WhenAll(allAplicationInstancesTasks);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                _scenarioFailedErrors.Clear();
                foreach (var aplicationInstanceTask in allAplicationInstancesTasks)
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

                    foreach (var scenarioSuiteApplicationInstance in _scenarioSuiteApplicationInstances)
                    {
                        foreach (var step in scenarioSuiteApplicationInstance.Instance.Steps)
                        {
                            if (!_allStepsIdsToExecute.Contains(step.Id) && (!step.PreviousStepsIds.Any() || step.PreviousStepsIds.All(stepId => _allStepsIdsToExecute.Contains(stepId))))
                            {
                                currentScenarioSuiteStepsStage.AddApplicationInstanceStep(scenarioSuiteApplicationInstance, step);
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
                foreach (var scenarioSuiteApplicationInstance in _scenarioSuiteApplicationInstances)
                {
                    var cyclicDependencySteps = scenarioSuiteApplicationInstance.Instance.Steps.Where(step => !_allStepsIdsToExecute.Contains(step.Id));
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
                    var currentStageApplicationSteps = _stepsExecutionStages.Dequeue();
                    await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Running step execution stage with steps {currentStageApplicationSteps}");
                    var currentStageExecuteStepsTasks = new List<Task<List<RunScenarioStepOperationResult?>>>();
                    foreach (var currentStageApplicationStep in currentStageApplicationSteps.InstanceHandlersSteps)
                    {
                        currentStageExecuteStepsTasks.Add(RunStepsForApplicationInstanceAsync(currentStageApplicationStep, scenarioData, token));
                    }
                    await Task.WhenAll(currentStageExecuteStepsTasks);
                    await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Finished running step execution stage with steps {currentStageApplicationSteps}");

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

        private async Task<List<RunScenarioStepOperationResult?>> RunStepsForApplicationInstanceAsync(KeyValuePair<ScenarioSuiteTestEngineInstanceHandler, List<ScenarioStep>> applicationInstanceSteps, ScenarioData scenarioData, CancellationToken token)
        {
            var allStepsResults = new List<RunScenarioStepOperationResult?>();
            foreach (var instanceStep in applicationInstanceSteps.Value)
            {
                var result = await applicationInstanceSteps.Key.ExecuteStepAsync(instanceStep, _scenarioSuiteData!, scenarioData, token);
                allStepsResults.Add(result as RunScenarioStepOperationResult);
            }
            return allStepsResults;
        }

        protected virtual IEnumerable<FieldInfo> GetApplicationInstancesMembers()
        {
            if (_scenarioSuiteType == null || _mainTestEngineRunContext == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var applicationInstanceInterfaceType = typeof(IInstance);
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

        protected virtual async Task<OperationResult?> StartApplicationInstanceAsync(IInstance instance, CancellationToken token)
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
                _scenarioFailedErrors.Add($"Application instance {instance.Id} failed to start with exception {ex}");
                return null;
            }
            await _mainTestEngineRunContext.LogMessage(LogLevel.Informational, $"Running runner in instance {instance.Id}");
            _scenarioSuiteApplicationInstances.Add(scenarioSuiteTestEngineApplicationInstance);
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
