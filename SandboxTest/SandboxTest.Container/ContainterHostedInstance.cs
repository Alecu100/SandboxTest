using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using System.IO;
using System.Net;
using System.Text;

namespace SandboxTest.Container
{
    public class ContainterHostedInstance : InstanceBase, IHostedInstance
    {
        protected const string DockerFileFormat = @"
FROM {0} AS base
USER app

{1}

{2}

COPY . .
ENTRYPOINT [""dotnet"", ""SandboxTest.Container.exe""]
";

        protected DockerClient? _dockerClient;
        protected IHostedInstanceMessageChannel? _messageChannel;
        protected List<string>? _addresses;
        protected string _baseImage;
        protected HashSet<KeyValuePair<string, string>> _exposedPorts = new HashSet<KeyValuePair<string, string>>();
        protected HashSet<KeyValuePair<string, string>> _environmentVariables = new HashSet<KeyValuePair<string, string>>();
        protected Func<ContainterHostedInstance, IHostedInstanceContext, Task>? _configureBuildFunc;
        protected Func<IReadOnlyList<IPAddress>, ICollection<IPAddress>>? _ipAddressesFilterFunc;
        protected IDictionary<string, AuthConfig> _authConfigs = new Dictionary<string, AuthConfig>();
        protected Credentials? _credentials;
        protected string? _registryAddress;
        protected int _dockerImageSuffix;
        protected string? _dockerFileFullName;
        protected string? _imageName;
        protected string? _containerId;

        public ContainterHostedInstance(string id) : base(id)
        {
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
        public IReadOnlyList<string> Addresses { get => _addresses ?? throw new InvalidOperationException("Container host not started"); }

        /// <summary>
        /// Gets the exposed ports collection allowing new ports to be exposed.
        /// </summary>
        public ICollection<KeyValuePair<string, string>> ExposedPorts { get => _exposedPorts; }

        /// <summary>
        /// Gets the environment variables collection allowing new environment variables to be added to the container.
        /// </summary>
        public ICollection<KeyValuePair<string, string>> EnvironmenVariables { get => _environmentVariables; }

        /// <summary>
        /// Gets or sets the base image used to build the container.
        /// </summary>
        public string BaseImage { get => _baseImage; set => _baseImage = value; }

        /// <summary>
        /// Gets or sets the ip address filter to filter out unwanted ip addresses from using them.
        /// </summary>
        public Func<IReadOnlyList<IPAddress>, ICollection<IPAddress>>? IpAddressesFilterFunc { get => _ipAddressesFilterFunc; }

        /// <summary>
        /// Gets or sets the credentials used to connect to the provided docker registry.
        /// </summary>
        public Credentials? Credentials { get => _credentials; set => _credentials = value; }

        /// <summary>
        /// Gets or sets the docker containter registry address used to get base containers from.
        /// </summary>
        public string? RegistryAddress { get => _registryAddress; set => _registryAddress = value; }

        /// <summary>
        /// Gets the authentication configuration for docker.
        /// </summary>
        public IDictionary<string, AuthConfig> AuthConfig { get => _authConfigs; }


        [AttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, nameof(StartAsync), -300)]
        public async Task ConfigureBuildAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            if (_configureBuildFunc != null)
            {
                await _configureBuildFunc(this, instanceContext);
            }
            if (_credentials != null && _registryAddress != null)
            {
                _dockerClient = new DockerClientConfiguration(new Uri(_registryAddress), _credentials)
                     .CreateClient();
                return;
            }
            _dockerClient = new DockerClientConfiguration()
                .CreateClient();

            await GenerateDockerFile();
        }


        [AttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, nameof(StartAsync), -200)]
        public virtual async Task BuildAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            if (_dockerClient == null || _dockerFileFullName == null)
            {
                throw new InvalidOperationException("Configure build not ran");
            }

            var imageContents = await GenerateImageContents();
            _imageName = $"sandbox-test.{_id.ToLowerInvariant()}.{_dockerImageSuffix}";
            await _dockerClient.Images.BuildImageFromDockerfileAsync(
                new ImageBuildParameters { AuthConfigs = _authConfigs, Dockerfile = _dockerFileFullName, Tags = new string[] { _imageName } }, 
                imageContents, _authConfigs.Values, null,  new ContainerBuildProgress(json => Task.CompletedTask));
            await imageContents.DisposeAsync();
        }

        public virtual async Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            if (_dockerClient == null || _imageName == null)
            {
                throw new InvalidOperationException("Image not built");
            }
            var createContainerResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters { Image =  _imageName, Name = Id });
            _containerId = createContainerResponse.ID;
            await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        /// <summary>
        /// Stops the container and removes it along with the built image.
        /// </summary>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task StopAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData)
        {
            if (_dockerClient == null || _containerId == null)
            {
                throw new InvalidOperationException("Container not started");
            }
            await _dockerClient.Containers.StopContainerAsync(_containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
            await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters { Force = true, RemoveLinks = true, RemoveVolumes = true });
            await _dockerClient.Images.DeleteImageAsync(_imageName, new ImageDeleteParameters { Force = true });
        }

        public void UseMessageChannel(IHostedInstanceMessageChannel messageChannel)
        {
            _messageChannel = messageChannel;
        }

        public IHostedInstance OnConfigureBuild()
        {
            return this;
        }

        protected virtual async Task GenerateDockerFile()
        {
            _dockerImageSuffix = Environment.CurrentDirectory.GetHashCode();
            _dockerFileFullName = $"{Environment.CurrentDirectory.Trim('\\', '/')}\\DockerFile.SandboxTest.{_id}.{_dockerImageSuffix}";
            var dockerFileEnvironmentVariablesSection = string.Join(Environment.NewLine, _environmentVariables.Select(environmentVariable => $"ENV {environmentVariable.Key}={environmentVariable.Value}"));
            var dockerFileExposedPortsSection = string.Join(Environment.NewLine, _exposedPorts.Select(exposedPort => $"EXPOSE {exposedPort.Key}:{exposedPort.Value}"));
            var dockerFileContent = string.Format(DockerFileFormat, _baseImage, dockerFileEnvironmentVariablesSection, dockerFileExposedPortsSection);
            await File.WriteAllTextAsync(_dockerFileFullName, dockerFileContent);
        }

        protected virtual async Task<Stream> GenerateImageContents()
        {
            var tarStream = new MemoryStream();
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);

            using var tarArchiveOutputStream = new TarOutputStream(tarStream, Encoding.UTF8)
            {
                IsStreamOwner = false
            };

            foreach (var file in files)
            {
                var tarFileFullName = file.Substring(Environment.CurrentDirectory.Length).Replace('\\', '/').TrimStart('/');

                var fileTarEntry = TarEntry.CreateTarEntry(tarFileFullName);
                using var fileStream = File.OpenRead(file);
                fileTarEntry.Size = fileStream.Length;
                await tarArchiveOutputStream.PutNextEntryAsync(fileTarEntry, default);

                //Now write the bytes of data
                byte[] localBuffer = new byte[32 * 1024];
                while (true)
                {
                    var numRead = await fileStream.ReadAsync(localBuffer, 0, localBuffer.Length);
                    if (numRead <= 0)
                        break;

                    await tarArchiveOutputStream.WriteAsync(localBuffer, 0, numRead);
                }

                await tarArchiveOutputStream.CloseEntryAsync(default);
            }
            tarArchiveOutputStream.Close();

            //Reset the stream and return it, so it can be used by the caller
            tarStream.Position = 0;
            return tarStream;
        }
    }
}
