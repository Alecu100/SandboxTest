using Newtonsoft.Json;
using SandboxTest.Engine.ChildTestEngine;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using SandboxTest.Instance;
using SandboxTest.Instance.Hosted;
using SandboxTest.Scenario;
using System.Threading;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngineInstanceHandler
    {
        private readonly Guid _runId;
        private readonly IInstance _assignedInstance;
        private IHostedInstance? _assignedHostedInstance;
        private readonly IMainTestEngineRunContext _mainTestEngineRunContext;
        private readonly Type _scenarioSuiteType;
        private HostedInstanceData? _hostedInstanceData;
        private IChildTestEngine? _childTestEngine;
        private string? _mainAssemblyPath;
        private string? _assemblySourceName;
        private string? _mainPath;

        /// <summary>
        /// Returns the current application instance assigned to the scenario suite application instance that is not running used to keep track of all the steps.
        /// </summary>
        public IInstance Instance { get { return _assignedInstance; } }

        public ScenarioSuiteTestEngineInstanceHandler(Guid runId, IInstance instance, Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext)
        {
            _runId = runId;
            _assignedInstance = instance;
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteType = scenarioSuiteType; 
        }

        /// <summary>
        /// Starts the actual application instance process and configures messaging for it.
        /// </summary>
        /// <returns></returns>
        public async Task LoadInstanceAsync(CancellationToken token)
        {
            _mainAssemblyPath = _scenarioSuiteType.Assembly.Location;
            _assemblySourceName = Path.GetFileName(_mainAssemblyPath);
            _mainPath = Path.GetDirectoryName(_mainAssemblyPath)!;

            if (_assignedInstance is IHostedInstance)
            {
                _hostedInstanceData = new HostedInstanceData
                {
                    RunId = _runId,
                    ApplicationInstanceId = _assignedInstance.Id,
                    AssemblySourceName = _assemblySourceName,
                    MainPath = _mainPath!,
                    HostedInstanceInitializerAssemblyFullName = typeof(HostedInstanceInitializer).Assembly.Location,
                    HostedInstanceInitializerTypeFullName = typeof(HostedInstanceInitializer).FullName!,
                    ScenarioSuiteTypeFullName = _scenarioSuiteType.FullName!
                };

                _assignedHostedInstance = (IHostedInstance)_assignedInstance;
                await _assignedHostedInstance.StartAsync(_mainTestEngineRunContext, _hostedInstanceData, token);
                await _assignedHostedInstance.MessageChannel!.StartAsync(_assignedInstance.Id, _runId, false);
            }
            else
            {
                _childTestEngine = new ChildTestEngine.ChildTestEngine();
                await _childTestEngine.LoadInstanceAsync($"{_mainPath}\\{_assemblySourceName}", _scenarioSuiteType.FullName!, _assignedInstance.Id);
            }
        }

        /// <summary>
        /// Stops the actual application instance process.
        /// </summary>
        /// <returns></returns>
        public async Task StopInstanceAsync()
        {
            if (_assignedHostedInstance != null && _hostedInstanceData != null)
            {
                var operation = new StopInstanceOperation(_assignedInstance.Id);
                await ExecuteHostedInstanceOperationAsync(operation, default);
                await _assignedHostedInstance.StopAsync(_mainTestEngineRunContext, _hostedInstanceData);
                return;
            }

            if (_childTestEngine == null)
            {
                throw new InvalidOperationException("No child engine assigned");
            }
            await _childTestEngine.StopInstanceAsync();
        }

        /// <summary>
        /// Executes a specific step for an instance.
        /// </summary>
        /// <param name="scenarioStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStep scenarioStep, ScenarioStepData stepContext, CancellationToken cancellationToken)
        {
            if (_assignedHostedInstance != null)
            {
                var operation = new RunScenarioStepOperation(scenarioStep.Id, stepContext);
                return await ExecuteHostedInstanceOperationAsync(operation, cancellationToken);
            }

            if (_childTestEngine == null)
            {
                throw new InvalidOperationException("No child engine assigned");
            }
            return await _childTestEngine.RunStepAsync(scenarioStep.Id, stepContext);
        }

        /// <summary>
        /// Resets an instance of all the steps, preparing it to run a new scenario.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ResetInstanceAsync(CancellationToken cancellationToken)
        {
            await _assignedInstance.ResetAsync();

            if (_assignedHostedInstance != null)
            {
                var operation = new ResetInstanceOperation(_assignedInstance.Id);
                return await ExecuteHostedInstanceOperationAsync(operation, cancellationToken);
            }

            if (_childTestEngine == null)
            {
                throw new InvalidOperationException("No child engine assigned");
            }
            return await _childTestEngine.ResetInstanceAsync();
        }

        public async Task<OperationResult?> RunInstanceAsync(CancellationToken cancellationToken)
        {
            if (_assignedHostedInstance != null)
            {
                var operation = new RunInstanceOperation(_assignedInstance.Id);
                return await ExecuteHostedInstanceOperationAsync(operation, cancellationToken);
            }

            if (_childTestEngine == null)
            {
                throw new InvalidOperationException("No child engine assigned");
            }
            return await _childTestEngine.RunInstanceAsync();
        }

        /// <summary>
        /// Loads a scenarion to be able to run it, initializing all the steps.
        /// </summary>
        /// <param name="scenarioName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> LoadScenarioAsync(string scenarioName, CancellationToken cancellationToken)
        {
            if (_assignedHostedInstance != null)
            {
                var operation = new LoadScenarioOperation(scenarioName);
                return await ExecuteHostedInstanceOperationAsync(operation, cancellationToken);
            }

            if (_childTestEngine == null)
            {
                throw new InvalidOperationException("No child engine assigned");
            }
            return await _childTestEngine.LoadScenarioAsync(scenarioName);
        }

        private async Task<OperationResult?> ExecuteHostedInstanceOperationAsync(Operation operation, CancellationToken cancellationToken)
        {
            try
            {
                if (_assignedHostedInstance == null)
                {
                    throw new InvalidOperationException("Assigned instance is not a hosted instance to send messages to");
                }
                if (_assignedHostedInstance.MessageChannel == null)
                {
                    throw new InvalidOperationException("Assigned instance does not have a message channel assigned");
                }
                var json = JsonConvert.SerializeObject(operation, JsonUtils.JsonSerializerSettings);
                await _assignedHostedInstance.MessageChannel.SendMessageAsync(json);

                var operationResult = JsonConvert.DeserializeObject<OperationResult>(await _assignedHostedInstance.MessageChannel.ReceiveMessageAsync(), JsonUtils.JsonSerializerSettings);
                return operationResult;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error sending operation of type {operation.GetType().Name} to application instance {_assignedInstance.Id} {ex}");
            }
        }

        /// <summary>
        /// Compares only if they both are associated to the same application instance id.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not ScenarioSuiteTestEngineInstanceHandler)
            {
                return false;
            }
            
            var otherHandler = obj as ScenarioSuiteTestEngineInstanceHandler;
            if (otherHandler?._assignedInstance?.Id == null)
            {
                return false;
            }
            return otherHandler._assignedInstance.Id.Equals(_assignedInstance.Id);
        }

        /// <summary>
        /// Returns only the hashcode of the application instance id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _assignedInstance.Id.GetHashCode();
        }
    }
}
