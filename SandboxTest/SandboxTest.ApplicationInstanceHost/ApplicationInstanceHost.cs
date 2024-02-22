using SandboxTest.Engine.ChildTestEngine;
using SandboxTest.Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest.ApplicationInstanceHost
{
    public class ApplicationInstanceHost
    {
        private readonly IChildTestEngine _childTestEngine;
        private readonly Guid _runId;
        private NamedPipeServerStream? _pipeServerStream;
        private string _testSource;
        private string _scenarionSuitName;
        private string _scenarionName;
        private string _applicationInstanceId;

        public ApplicationInstanceHost(Guid runId, string applicationInstanceId, string testSource, string scenarionSuitName, string scenarionName) 
        {
            _runId = runId;
            _testSource = testSource;
            _scenarionSuitName = scenarionSuitName;
            _scenarionName = scenarionName;
            _childTestEngine = new ChildTestEngine();
            _applicationInstanceId = applicationInstanceId;
        }

        public async Task InitializeAsync()
        {
            _pipeServerStream = new NamedPipeServerStream(PipeUtils.GetChildApplicationInstanceHostPipeName(_runId, _applicationInstanceId), PipeDirection.InOut, 3, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }
    }
}
