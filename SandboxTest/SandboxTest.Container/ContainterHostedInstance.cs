using Docker.DotNet;
using Docker.DotNet.Models;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;

namespace SandboxTest.Container
{
    public class ContainterHostedInstance : InstanceBase, IHostedInstance
    {
        private const string DockerFileFormat = @"
FROM {0} AS base
USER app


{1}

{2}

COPY . .
ENTRYPOINT [""dotnet"", ""SandboxTest.Container.exe""]
";


        private readonly DockerClient _dockerClient;

        private IHostedInstanceMessageChannel? _messageChannel;

        private string? _address;

        private string _baseImage;

        private HashSet<KeyValuePair<string, string>> _exposedPorts;

        public ContainterHostedInstance(string id) : base(id)
        {
            _dockerClient = new DockerClientConfiguration()
                 .CreateClient();
            _exposedPorts = new HashSet<KeyValuePair<string, string>>();
#if NET6_0
            _baseImage = "mcr.microsoft.com/dotnet/aspnet:6.0";
#elif NET7_0
            _baseImage = "mcr.microsoft.com/dotnet/aspnet:7.0";
#elif NET8_0
            _baseImage = "mcr.microsoft.com/dotnet/aspnet:8.0";
#endif
        }

        /// <summary>
        /// Gets message channel used by the container hosted instance.
        /// </summary>
        public IHostedInstanceMessageChannel? MessageChannel { get => _messageChannel; }

        /// <summary>
        /// Containers have an unique ip address that is filled in when the container is started.
        /// </summary>
        public string Address { get => _address ?? throw new InvalidOperationException("Container host not started"); }

        [AttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, nameof(StartAsync), -300)]
        public async Task ConfigureBuildAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {

        }


        [AttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, nameof(StartAsync), -200)]
        public async Task BuildAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {

        }

        public Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            //_dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters { ExposedPorts = new Dictionary<string, EmptyStruct> { { "8080:80", default } });
            throw new NotImplementedException();
        }

        public Task StopAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData)
        {
            throw new NotImplementedException();
        }

        public void UseMessageChannel(IHostedInstanceMessageChannel messageChannel)
        {
            _messageChannel = messageChannel;
        }

        public IHostedInstance OnConfigureBuild()
        {
            return this;
        }
    }
}
