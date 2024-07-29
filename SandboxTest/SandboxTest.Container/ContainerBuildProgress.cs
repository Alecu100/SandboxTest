using Docker.DotNet.Models;

namespace SandboxTest.Container
{
    public class ContainerBuildProgress : IProgress<JSONMessage>
    {
        private Func<JSONMessage, Task> _progressFunc;

        public ContainerBuildProgress(Func<JSONMessage, Task> progressFunc)
        {
            _progressFunc = progressFunc;
        }

        public void Report(JSONMessage value)
        {
            _progressFunc(value);
        }
    }
}
