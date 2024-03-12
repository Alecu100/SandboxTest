using SandboxTest.Engine.ChildTestEngine;
using SandboxTest.Engine.Utils;
using System.IO.Pipes;

namespace SandboxTest.Engine.ApplicationRunner
{
    public class ApplicationInstanceRunner
    {
        private readonly IChildTestEngine _childTestEngine;
        private readonly Guid _runId;
        private NamedPipeServerStream? _pipeServerStream;
        private string _testSource;
        private string _scenarioSuitName;
        private string _applicationInstanceId;

        public ApplicationInstanceRunner(Guid runId, string applicationInstanceId, string testSource, string scenarioSuitName)
        {
            _runId = runId;
            _testSource = testSource;
            _scenarioSuitName = scenarioSuitName;
            _childTestEngine = new ChildTestEngine.ChildTestEngine();
            _applicationInstanceId = applicationInstanceId;
        }

        public async Task InitializeAsync()
        {
            _pipeServerStream = new NamedPipeServerStream(PipeUtils.GetChildApplicationInstanceHostPipeName(_runId, _applicationInstanceId), PipeDirection.InOut, 3, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }
    }
}
