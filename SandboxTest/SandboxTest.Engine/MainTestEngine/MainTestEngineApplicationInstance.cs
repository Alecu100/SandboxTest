using SandboxTest.Engine.Utils;
using System.IO.Pipes;
using System.Text.Json;

namespace SandboxTest.Engine.MainTestEngine
{
    public class MainTestEngineApplicationInstance
    {
        private readonly Guid _runId;
        private readonly IApplicationInstance _applicationInstance;
        private Lazy<Task<NamedPipeClientStream>> _instancePipeStream;
        private byte[] _instancePipeStreamBuffer = new byte[2000];

        public MainTestEngineApplicationInstance(Guid runId, IApplicationInstance instance)
        {
            _runId = runId;
            _applicationInstance = instance;
            _instancePipeStream = new Lazy<Task<NamedPipeClientStream>>(async () =>
            {
                var pipe = new NamedPipeClientStream(".", PipeUtils.GetChildApplicationInstanceHostPipeName(_runId, _applicationInstance), PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                await pipe.ConnectAsync();
                return pipe;
            }); 
        }

        public Task ResetAsync() 
        { 
            return Task.CompletedTask;
        }

        public async Task<OperationResult?> ExecuteStepAsync(ScenarioStepId scenarioStepId)
        {
            var pipe = await _instancePipeStream.Value;
            await JsonSerializer.SerializeAsync(pipe, scenarioStepId);
            var stepExecutionResult = await JsonSerializer.DeserializeAsync<OperationResult>(pipe);
            return stepExecutionResult;
        }
    }
}
