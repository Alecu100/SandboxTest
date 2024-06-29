using SandboxTest.Engine.Operations;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class ChildTestEngine : IChildTestEngine
    {
        private Type? _scenarioSuiteType;
        private ScenariosAssemblyLoadContext? _scenariosAssemblyLoadContext;
        private Assembly? _scenarioSuiteAssembly;
        private IInstance? _runningInstance;
        private IHostedInstance? _runningHostedInstance;
        private object? _scenarioSuiteInstance;
        private IAttachedMethodsExecutor _attachedMethodsExecutor;

        public ChildTestEngine()
        {
            _attachedMethodsExecutor = new AttachedMethodsExecutor();
        }

        /// <inheritdoc/>
        public IInstance? RunningInstance { get => _runningInstance; }

        public async virtual Task<OperationResult> LoadScenarioAsync(string scenarioMethodName)
        {
            if (_scenarioSuiteType == null)
            {
                return new OperationResult(false, "No scenario suite loaded");
            }
            if (_runningInstance == null)
            {
                return new OperationResult(false, "No application instance running");
            }

            var scenarioMethod = _scenarioSuiteType.GetMethod(scenarioMethodName, BindingFlags.Instance | BindingFlags.Public);
            if (scenarioMethod == null) 
            {
                return new OperationResult(false, $"Could not find scenario method with name {scenarioMethodName}");
            }
            if (scenarioMethod.GetParameters().Length > 0)
            {
                return new OperationResult(false, $"Scenario method with name {scenarioMethodName} has parameters defined");
            }
            try
            {
                var result = scenarioMethod.Invoke(_scenarioSuiteInstance, null);
                var resultTask = result as Task;
                if (resultTask != null)
                {
                    await resultTask;
                }
                return new OperationResult(true);
            }
            catch (Exception ex) 
            {
                return new OperationResult(false, $"Error {ex} running the scenarion method with name {scenarioMethodName} ");
            }
        }

        public async virtual Task<OperationResult> ResetInstanceAsync()
        {
            if (_runningInstance == null)
            {
                return new OperationResult(false, "No application instance running");
            }

            try
            {
                var allInstancesToRun = new List<object>();
                allInstancesToRun.AddRange(_runningInstance.Controllers);
                allInstancesToRun.Add(_runningInstance.Runner!);
                await _attachedMethodsExecutor.ExecutedMethods(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runningInstance.ResetAsync, new object[] { _runningInstance.Runner });

                return new OperationResult(true);
            }
            catch(Exception ex)
            {
                return new OperationResult(false, $"Error reseting the application instance {ex}");
            }
        }

        public virtual Task<OperationResult> LoadInstanceAsync(string sourceAssemblyNameFulPath, string scenarioSuiteTypeFullName, string applicationInstanceId)
        {
            try
            {
                _scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(sourceAssemblyNameFulPath);
                _scenarioSuiteAssembly = _scenariosAssemblyLoadContext.LoadFromAssemblyPath(sourceAssemblyNameFulPath);
                foreach (var assemblyType in _scenarioSuiteAssembly.GetTypes())
                {
                    if (assemblyType.FullName == scenarioSuiteTypeFullName)
                    {
                        _scenarioSuiteType = assemblyType;
                        break;
                    }
                }

                if (_scenarioSuiteType == null)
                {
                    throw new InvalidOperationException($"Could not find scenario suite type {scenarioSuiteTypeFullName} to load application instance from");
                }
                if (_scenarioSuiteType.GetConstructors().All(constructor => constructor.GetParameters().Length > 0))
                {
                    throw new InvalidOperationException($"Scenario suite type {scenarioSuiteTypeFullName} does not contain a constructor without parameters");
                }
                _scenarioSuiteInstance = Activator.CreateInstance(_scenarioSuiteType);

                var allFields = new List<FieldInfo>();
                allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.Public));
                allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
                foreach (var field in allFields) 
                {
                    if (field.FieldType.IsAssignableTo(typeof(IInstance)))
                    {
                        var applicationInstance = field.GetValue(_scenarioSuiteInstance) as IInstance;
                        if (applicationInstance != null && applicationInstance.Id == applicationInstanceId)
                        {
                            _runningInstance = applicationInstance;
                            if (_runningInstance is IHostedInstance)
                            {
                                _runningHostedInstance = (IHostedInstance)_runningInstance;
                            }
                            break;
                        }
                    }
                }

                return Task.FromResult(new OperationResult(true));
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
                return Task.FromResult(new OperationResult(false, ex.ToString()));
            }
        }

        public async virtual Task<OperationResult> RunInstanceAsync()
        {
            try
            {
                if (_runningInstance == null)
                {
                    throw new InvalidOperationException($"No running instance assigned");
                }
                if (_runningInstance.Runner == null)
                {
                    throw new InvalidOperationException($"Instance has no runner assigned");
                }
                var allInstancesToRun = new List<object>();
                allInstancesToRun.AddRange(_runningInstance.Controllers);
                allInstancesToRun.Add(_runningInstance.Runner);
                await _attachedMethodsExecutor.ExecutedMethods(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runningInstance.Runner.RunAsync, new object[] { _runningInstance.Runner });
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new OperationResult(false, ex.ToString());
            }
        }

        public async virtual Task<OperationResult> RunStepAsync(ScenarioStepId stepId, ScenarioStepData stepContext)
        {
            if (_runningInstance == null)
            {
                return new RunScenarioStepOperationResult(false, stepContext, "No application instance running");
            }
            var step = _runningInstance.Steps.FirstOrDefault(step => step.Id.ApplicationInstanceId == _runningInstance.Id && ((step.Id.Name != null && stepId.Name == step.Id.Name) || (step.Id.StepIndex == stepId.StepIndex)));
            if (step == null)
            {
                return new RunScenarioStepOperationResult(false, stepContext, "Step not found");
            }
            try
            {
                await step.RunAsync(stepContext);
                return new RunScenarioStepOperationResult(true, stepContext);
            }
            catch (Exception ex)
            {
                return new RunScenarioStepOperationResult(false, stepContext, $"Error running step {ex}");
            }
        }

        public async Task<OperationResult> StopInstanceAsync()
        {
            if (_scenarioSuiteType == null || _runningInstance == null)
            {
                throw new InvalidOperationException($"No instance assigned and running");
            }
            if (_runningInstance.Runner == null)
            {
                throw new InvalidOperationException($"Could application instance with id {_runningInstance.Id} in scenario suite type {_scenarioSuiteType.FullName} does not have an assigned runner");
            }
            var allInstancesToRun = new List<object>();
            allInstancesToRun.AddRange(_runningInstance.Controllers);
            allInstancesToRun.Add(_runningInstance.Runner);
            await _attachedMethodsExecutor.ExecutedMethods(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runningInstance.Runner.StopAsync, new object[] { _runningInstance.Runner });
            return new OperationResult(true);
        }
    }
}
