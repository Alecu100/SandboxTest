using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using System.IO.Pipes;
using System.Text;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngineApplicationInstance
    {
        private readonly Guid _runId;
        private readonly IApplicationInstance _applicationInstance;
        private Lazy<Task<NamedPipeClientStream>> _instancePipeStream;
        private byte[] _instancePipeStreamBuffer = new byte[2000];

        public ScenarioSuiteTestEngineApplicationInstance(Guid runId, IApplicationInstance instance)
        {
            _runId = runId;
            _applicationInstance = instance;
            _instancePipeStream = new Lazy<Task<NamedPipeClientStream>>(async () =>
            {
                var pipe = new NamedPipeClientStream(".", PipeUtils.GetChildApplicationInstanceHostPipeName(_runId, _applicationInstance.Id), PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                await pipe.ConnectAsync();
                return pipe;
            }); 
        }

        public Task ResetAsync() 
        { 
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes a specific step for an application instance.
        /// </summary>
        /// <param name="scenarioStepId"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStepId scenarioStepId)
        {
            var operation = new RunScenarioStepOperation(scenarioStepId);
            return await ExecuteOperationAsync(operation);
        }

        /// <summary>
        /// Resets an application instance and all the executed steps, preparing it to run a new scenario.
        /// </summary>
        /// <param name="applicationInstanceId"></param>
        /// <returns></returns>
        public async Task<OperationResult?> ResetInstanceAsync(string applicationInstanceId)
        {
            var operation = new ResetApplicationInstanceOperation(applicationInstanceId);
            return await ExecuteOperationAsync(operation);
        }

        /// <summary>
        /// Loads a scenarion to be able to run it, initializing all the steps.
        /// </summary>
        /// <param name="scenarionName"></param>
        /// <returns></returns>
        public async Task<OperationResult?> LoadScenarionAsync(string scenarionName)
        {
            var operation = new LoadScenarionOperation(scenarionName);
            return await ExecuteOperationAsync(operation);
        }

        private async Task<OperationResult?> ExecuteOperationAsync(Operation operation)
        {
            var pipeStream = await _instancePipeStream.Value;
            var json = JsonConvert.SerializeObject(operation, PipeUtils.PipeJsonSerializerSettings);
            await pipeStream.WriteAsync(Encoding.UTF8.GetBytes(json));
            int bytesRead;
            var offset = 0;
            do
            {
                bytesRead = await pipeStream.ReadAsync(_instancePipeStreamBuffer, offset, _instancePipeStreamBuffer.Length - offset);
                offset += bytesRead;


            }
            while (bytesRead > 0);
            var operationResult = JsonConvert.DeserializeObject<OperationResult>(Encoding.UTF8.GetString(_instancePipeStreamBuffer, 0, offset));
            return operationResult;
        }
    }
}
