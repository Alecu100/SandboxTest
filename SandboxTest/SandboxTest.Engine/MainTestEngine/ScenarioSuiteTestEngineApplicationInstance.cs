using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Text;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngineApplicationInstance
    {
        private readonly Guid _runId;
        private readonly IApplicationInstance _applicationInstance;
        private readonly IMainTestEngineRunContext _mainTestEngineRunContext;
        private readonly Type _scenarioSuiteType;
        private Process? _applicationInstanceProcess;

        public ScenarioSuiteTestEngineApplicationInstance(Guid runId, IApplicationInstance instance, Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext)
        {
            _runId = runId;
            _applicationInstance = instance;
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteType = scenarioSuiteType; 
        }

        public async Task StartInstanceAsync()
        {
            var mainAssemblyPath = _scenarioSuiteType.Assembly.Location;
            var assemblyName = Path.GetFileName(mainAssemblyPath);
            var mainPath = Path.GetDirectoryName(mainAssemblyPath);
            var applicationRunnerPath = $"{mainPath}\\SandboxTest.Engine.ApplicationRunner.exe";
            var arguments = $"-mainPath=\"{mainPath}\"  -assemblyName=\"{assemblyName}\"  -scenarioSuiteType=\"{_scenarioSuiteType}\"  -runId=\"{_runId}\" ";
            _applicationInstanceProcess = await _mainTestEngineRunContext.LaunchProcessAsync(applicationRunnerPath, _mainTestEngineRunContext.IsBeingDebugged, mainPath, arguments);
            await _applicationInstance.MessageSink.StartAsync(_applicationInstance.Id, _runId, false);
        }

        /// <summary>
        /// Executes a specific step for an application instance.
        /// </summary>
        /// <param name="scenarioStepId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStepId scenarioStepId, CancellationToken cancellationToken)
        {
            var operation = new RunScenarioStepOperation(scenarioStepId);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        /// <summary>
        /// Resets an application instance and all the executed steps, preparing it to run a new scenario.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ResetInstanceAsync(string applicationInstanceId, CancellationToken cancellationToken)
        {
            var operation = new ResetApplicationInstanceOperation(applicationInstanceId);
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
            var operation = new LoadScenarionOperation(scenarioName);
            return await ExecuteOperationAsync(operation, cancellationToken);
        }

        private async Task<OperationResult?> ExecuteOperationAsync(Operation operation, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(operation, PipeUtils.PipeJsonSerializerSettings);
            await _applicationInstance.MessageSink.SendMessageAsync(json);

            var operationResult = JsonConvert.DeserializeObject<OperationResult>(await _applicationInstance.MessageSink.ReceiveMessageAsync());
            return operationResult;
        }
    }
}
