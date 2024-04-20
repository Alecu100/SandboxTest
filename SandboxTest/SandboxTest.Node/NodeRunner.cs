using SandboxTest.Executable;

namespace SandboxTest.Node
{
    public class NodeRunner : ExecutableRunner, INodeRunner
    {
        public NodeRunner(string executablePath, Func<string?, bool> isRunningFunc, string? workDirectory = null, List<string>? executableArguments = null, IDictionary<string, string>? environmentVariables = null) : base(executablePath, isRunningFunc, workDirectory, executableArguments, environmentVariables)
        {
        }

        public string BaseUrl => throw new NotImplementedException();

        public int Port => throw new NotImplementedException();

        public NodeServerTypes ServerType => throw new NotImplementedException();

        public string Url => throw new NotImplementedException();
    }
}
