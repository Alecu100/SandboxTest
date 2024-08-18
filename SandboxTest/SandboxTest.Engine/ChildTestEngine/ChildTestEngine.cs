using SandboxTest.Engine.Operations;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Internal;
using SandboxTest.Loader;
using SandboxTest.Scenario;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class ChildTestEngine : IChildTestEngine
    {
        private Type? _scenarioSuiteType;
        private ScenariosAssemblyLoadContext _scenariosAssemblyLoadContext;
        private Assembly? _scenarioSuiteAssembly;
        private IInstance? _instance;
        private object? _scenarioSuiteInstance;
        private IAttachedMethodsExecutor _attachedMethodsExecutor;
        private ScenarioSuiteContext? _scenarioSuiteContext;

        public ChildTestEngine(ScenariosAssemblyLoadContext scenariosAssemblyLoadContext)
        {
            _scenariosAssemblyLoadContext = scenariosAssemblyLoadContext;
            _attachedMethodsExecutor = new AttachedMethodsExecutor();
        }

        /// <inheritdoc/>
        public IScenarioSuiteContext? ScenarioSuiteContext { get => _scenarioSuiteContext; }

        /// <inheritdoc/>
        public IInstance? RunningInstance { get => _instance; }

        /// <inheritdoc/>
        public IAttachedMethodsExecutor AttachedMethodsExecutor { get => _attachedMethodsExecutor; }

        public async virtual Task<OperationResult> LoadScenarioAsync(string scenarioMethodName)
        {
            if (_scenarioSuiteType == null)
            {
                return new OperationResult(false, "No scenario suite loaded");
            }
            if (_instance == null)
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

        public async virtual Task<OperationResult> ResetInstanceAsync(ScenarioSuiteData scenarioSuiteData)
        {
            if (_instance == null|| _instance.Runner == null)
            {
                return new ScenarioSuiteOperationResult(false, scenarioSuiteData, "No application instance running");
            }

            try
            {
                RefreshScenarioSuiteContext(scenarioSuiteData);
                if (_instance.Runner.IsRunning)
                {
                    var allInstancesToRun = new List<object>();
                    allInstancesToRun.AddRange(_instance.Controllers);
                    allInstancesToRun.Add(_instance.Runner!);
                    await _attachedMethodsExecutor.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _instance.ResetAsync, new object[] { _instance.Runner, new ScenarioSuiteContext(scenarioSuiteData) });
                }

                return new ScenarioSuiteOperationResult(true, scenarioSuiteData);
            }
            catch(Exception ex)
            {
                return new ScenarioSuiteOperationResult(false, scenarioSuiteData, $"Error reseting the application instance {ex}");
            }
        }

        public virtual Task<OperationResult> LoadInstanceAsync(string sourceAssemblyNameFulPath, string scenarioSuiteTypeFullName, string instanceId)
        {
            try
            {
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
                        var instance = field.GetValue(_scenarioSuiteInstance) as IInstance;
                        if (instance != null && instance.Id == instanceId)
                        {
                            _instance = instance;
                            break;
                        }
                    }
                }
                if (_instance == null)
                {
                    throw new InvalidOperationException($"Could not find instance with id {instanceId}, id of an instance should be the same accross multiple scenario runs");
                }
                foreach (var controller in _instance.Controllers)
                {
                    if (controller is IRuntimeContextAccessor)
                    {
                        var runtimeContextAccessor = (IRuntimeContextAccessor)controller;
                        runtimeContextAccessor.InitializeContext(new RuntimeContext(this));
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

        public async virtual Task<OperationResult> RunInstanceAsync(ScenarioSuiteData scenarioSuiteData)
        {
            try
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"No running instance assigned");
                }
                if (_instance.Runner == null)
                {
                    throw new InvalidOperationException($"Instance has no runner assigned");
                }
                RefreshScenarioSuiteContext(scenarioSuiteData);
                var allInstancesToRun = new List<object>();
                allInstancesToRun.AddRange(_instance.Controllers);
                allInstancesToRun.Add(_instance.Runner);
                await _attachedMethodsExecutor.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _instance.Runner.RunAsync, new object[] { _instance.Runner, new ScenarioSuiteContext(scenarioSuiteData) });
                return new ScenarioSuiteOperationResult(true, scenarioSuiteData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ScenarioSuiteOperationResult(false, scenarioSuiteData, ex.ToString());
            }
        }

        public async virtual Task<OperationResult> RunStepAsync(ScenarioStepId stepId, ScenarioSuiteData scenarioSuiteData, ScenarioData scenarioData)
        {
            if (_instance == null)
            {
                return new RunScenarioStepOperationResult(false, scenarioSuiteData, scenarioData, "No application instance running");
            }
            var step = _instance.Steps.FirstOrDefault(step => step.Id.ApplicationInstanceId == _instance.Id && ((step.Id.Name != null && stepId.Name == step.Id.Name) || (step.Id.StepIndex == stepId.StepIndex)));
            if (step == null)
            {
                return new RunScenarioStepOperationResult(false, scenarioSuiteData, scenarioData, "Step not found");
            }
            try
            {
                RefreshScenarioSuiteContext(scenarioSuiteData);
                var scenarioStepRuntime = (IScenarioStepRuntime)step;
                await scenarioStepRuntime.RunAsync(new ScenarioStepContext(scenarioSuiteData, scenarioData));
                return new RunScenarioStepOperationResult(true, scenarioSuiteData, scenarioData);
            }
            catch (Exception ex)
            {
                return new RunScenarioStepOperationResult(false, scenarioSuiteData, scenarioData,  $"Error running step {ex}");
            }
        }

        public async Task<OperationResult> StopInstanceAsync(ScenarioSuiteData scenarioSuiteData)
        {
            if (_scenarioSuiteType == null || _instance == null)
            {
                throw new InvalidOperationException($"No instance assigned and running");
            }
            if (_instance.Runner == null)
            {
                throw new InvalidOperationException($"Could application instance with id {_instance.Id} in scenario suite type {_scenarioSuiteType.FullName} does not have an assigned runner");
            }
            RefreshScenarioSuiteContext(scenarioSuiteData);
            if (_instance.Runner.IsRunning)
            {
                var allInstancesToRun = new List<object>();
                allInstancesToRun.AddRange(_instance.Controllers);
                allInstancesToRun.Add(_instance.Runner);
                await _attachedMethodsExecutor.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _instance.Runner.StopAsync, new object[] { _instance.Runner, new ScenarioSuiteContext(scenarioSuiteData) });
            }

            return new ScenarioSuiteOperationResult(true, scenarioSuiteData);
        }

        private void RefreshScenarioSuiteContext(ScenarioSuiteData scenarioSuiteData)
        {
            _scenarioSuiteContext = new ScenarioSuiteContext(scenarioSuiteData);
        }
    }
}
