using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using System.Diagnostics;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngineApplicationInstance
    {
        private readonly Guid _runId;
        private readonly IApplicationInstance _instance;
        private readonly IMainTestEngineRunContext _mainTestEngineRunContext;
        private readonly Type _scenarioSuiteType;
        private Process? _applicationInstanceProcess;

        /// <summary>
        /// Returns the current application instance assigned to the scenario suite application instance.
        /// </summary>
        public IApplicationInstance Instance { get { return _instance; } }

        public ScenarioSuiteTestEngineApplicationInstance(Guid runId, IApplicationInstance instance, Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext)
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
            var mainAssemblyPath = _scenarioSuiteType.Assembly.Location;
            var assemblySourceName = Path.GetFileName(mainAssemblyPath);
            var mainPath = Path.GetDirectoryName(mainAssemblyPath);
            var applicationRunnerPath = $"{mainPath}\\SandboxTest.Engine.ApplicationContainer.exe";
            var arguments = $"-{Constants.MainPathArgument}=\"{mainPath}\"  -{Constants.AssemblySourceNameArgument}=\"{assemblySourceName}\"  " +
                $"-{Constants.ScenarioSuiteTypeFullNameArgument}=\"{_scenarioSuiteType.FullName}\"  -{Constants.RunIdArgument}=\"{_runId}\"  -{Constants.ApplicationInstanceIdArgument}=\"{_instance.Id}\"  ";
            _applicationInstanceProcess = await _mainTestEngineRunContext.LaunchProcessAsync(applicationRunnerPath, _mainTestEngineRunContext.IsBeingDebugged, mainPath, arguments);
            await _instance.MessageSink.ConfigureAsync(_instance.Id, _runId, false);
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
        /// <param name="scenarioStepId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStepId scenarioStepId, ScenarioStepContext stepContext, CancellationToken cancellationToken)
        {
            var operation = new RunScenarioStepOperation(scenarioStepId, stepContext);
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
                await _instance.MessageSink.SendMessageAsync(json);

                var operationResult = JsonConvert.DeserializeObject<OperationResult>(await _instance.MessageSink.ReceiveMessageAsync(), JsonUtils.JsonSerializerSettings);
                return operationResult;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, $"Error sending operation of type {operation.GetType().Name} to application instance {_instance.Id} {ex}");
            }
        }
    }
}
