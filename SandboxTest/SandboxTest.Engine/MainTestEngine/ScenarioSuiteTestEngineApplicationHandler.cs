using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using System.Diagnostics;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngineApplicationHandler
    {
        private readonly Guid _runId;
        private readonly IInstance _instance;
        private readonly IMainTestEngineRunContext _mainTestEngineRunContext;
        private readonly Type _scenarioSuiteType;
        private Process? _applicationInstanceProcess;

        /// <summary>
        /// Returns the current application instance assigned to the scenario suite application instance.
        /// </summary>
        public IInstance Instance { get { return _instance; } }

        public ScenarioSuiteTestEngineApplicationHandler(Guid runId, IInstance instance, Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext)
        {
            _runId = runId;
            _instance = instance;
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteType = scenarioSuiteType; 
        }

        /// <summary>
        /// Starts the actual application instance process and configures messaging for it.
        /// </summary>
        /// <returns></returns>
        public async Task StartInstanceAsync()
        {
            if (_instance is IHostedInstance)
            {
                
            }
            var mainAssemblyPath = _scenarioSuiteType.Assembly.Location;
            var assemblySourceName = Path.GetFileName(mainAssemblyPath);
            var mainPath = Path.GetDirectoryName(mainAssemblyPath);
            var applicationRunnerPath = $"{mainPath}\\SandboxTest.Engine.ApplicationContainer.exe";
            var arguments = $"-{Constants.MainPathArgument}=\"{mainPath}\"  -{Constants.AssemblySourceNameArgument}=\"{assemblySourceName}\"  " +
                $"-{Constants.ScenarioSuiteTypeFullNameArgument}=\"{_scenarioSuiteType.FullName}\"  -{Constants.RunIdArgument}=\"{_runId}\"  -{Constants.ApplicationInstanceIdArgument}=\"{_instance.Id}\"  ";
            _applicationInstanceProcess = await _mainTestEngineRunContext.LaunchProcessAsync(applicationRunnerPath, _mainTestEngineRunContext.IsBeingDebugged, mainPath, arguments);
            await _instance.MessageChannel.StartAsync(_instance.Id, _runId, false);
        }

        /// <summary>
        /// Stops the actual application instance process.
        /// </summary>
        /// <returns></returns>
        public async Task StopInstanceAsync()
        {
            if (_applicationInstanceProcess != null)
            {
                _applicationInstanceProcess.Kill(true);
                await _applicationInstanceProcess.WaitForExitAsync();
            }
        }

        /// <summary>
        /// Executes a specific step for an application instance.
        /// </summary>
        /// <param name="scenarioStep"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStep scenarioStep, ScenarioStepData stepContext, CancellationToken cancellationToken)
        {
            var operation = new RunScenarioStepOperation(scenarioStep.Id, stepContext);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        /// <summary>
        /// Resets an application instance and all the executed steps, preparing it to run a new scenario.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ResetInstanceAsync(CancellationToken cancellationToken)
        {
            await _instance.ResetAsync();
            var operation = new ResetApplicationInstanceOperation(_instance.Id);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        public async Task<OperationResult?> ReadyInstanceAsync(CancellationToken cancellationToken)
        {
            var operation = new ReadyOperation(_instance.Id);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        /// <summary>
        /// Loads a scenarion to be able to run it, initializing all the steps.
        /// </summary>
        /// <param name="scenarioName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> LoadScenarioAsync(string scenarioName, CancellationToken cancellationToken)
        {
            var operation = new LoadScenarioOperation(scenarioName);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        private async Task<OperationResult?> ExecuteOperationAsync(Operation operation, CancellationToken cancellationToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(operation, JsonUtils.JsonSerializerSettings);
                await _instance.MessageChannel.SendMessageAsync(json);

                var operationResult = JsonConvert.DeserializeObject<OperationResult>(await _instance.MessageChannel.ReceiveMessageAsync(), JsonUtils.JsonSerializerSettings);
                return operationResult;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error sending operation of type {operation.GetType().Name} to application instance {_instance.Id} {ex}");
            }
        }

        /// <summary>
        /// Compares only if they both are associated to the same application instance id.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not ScenarioSuiteTestEngineApplicationHandler)
            {
                return false;
            }
            
            var otherHandler = obj as ScenarioSuiteTestEngineApplicationHandler;
            if (otherHandler?._instance?.Id == null)
            {
                return false;
            }
            return otherHandler._instance.Id.Equals(_instance.Id);
        }

        /// <summary>
        /// Returns only the hashcode of the application instance id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _instance.Id.GetHashCode();
        }
    }
}
