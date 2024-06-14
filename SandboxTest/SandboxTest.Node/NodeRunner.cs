namespace SandboxTest.Node
{
    public class NodeRunner : INodeRunner
    {
        protected NodeServerTypes _serverType;

        public NodeRunner(NodeServerTypes serverType)
        {
        }

        public string BaseUrl => throw new NotImplementedException();

        public int Port => throw new NotImplementedException();

        public NodeServerTypes ServerType => throw new NotImplementedException();

        public string Url => throw new NotImplementedException();

        public Task BuildAsync()
        {
            throw new NotImplementedException();
        }

        public Task ConfigureBuildAsync()
        {
            throw new NotImplementedException();
        }

        public Task ConfigureRunAsync()
        {
            throw new NotImplementedException();
        }

        public Task ResetAsync()
        {
            throw new NotImplementedException();
        }

        public Task RunAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
