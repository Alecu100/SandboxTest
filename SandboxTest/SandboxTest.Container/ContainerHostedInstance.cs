using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using System.Text;

namespace SandboxTest.Container
{
    public class ContainerHostedInstance : InstanceBase, IHostedInstance
    {
        protected const string DockerFileFormat = @"
FROM {0} AS base
USER root

{1}

WORKDIR /app
COPY . .
ENTRYPOINT [""dotnet"", ""{2}.dll""]
";

        protected DockerClient? _dockerClient;
        protected IHostedInstanceMessageChannel? _messageChannel;
        protected List<string>? _addresses;
        protected string _baseImage;
        protected HashSet<KeyValuePair<short, short>> _exposedPorts = new HashSet<KeyValuePair<short, short>>();
        protected HashSet<KeyValuePair<string, string>> _environmentVariables = new HashSet<KeyValuePair<string, string>>();
        protected Func<ContainerHostedInstance, IHostedInstanceContext, Task>? _configureBuildFunc;
        protected IDictionary<string, AuthConfig> _authConfigs = new Dictionary<string, AuthConfig>();
        protected Credentials? _credentials;
        protected string? _registryAddress;
        protected string? _dockerFileName;
        protected string? _containerId;

        /// <summary>
        /// Creates an empty container hosted instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ContainerHostedInstance CreateEmptyInstance()
        {
            return new ContainerHostedInstance();
        }

        public ContainerHostedInstance()
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
        public ICollection<KeyValuePair<short, short>> ExposedPorts { get => _exposedPorts; }

        /// <summary>
        /// Gets the environment variables collection allowing new environment variables to be added to the container.
        /// </summary>
        public ICollection<KeyValuePair<string, string>> EnvironmenVariables { get => _environmentVariables; }

        /// <summary>
        /// Gets or sets the base image used to build the container.
        /// </summary>
        public string BaseImage { get => _baseImage; set => _baseImage = value; }

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

        /// <summary>
        /// Gets and sets whether the instance should be packaged in a separate dedicated folder.
        /// </summary>
        public bool IsPackaged { get; set; }

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

            await GenerateDockerFile(instanceData);
        }


        [AttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, nameof(StartAsync), -200)]
        public virtual async Task BuildAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            if (_dockerClient == null || _dockerFileName == null)
            {
                throw new InvalidOperationException("Configure build not ran");
            }

            using var imageContents = await GenerateImageContents();
            await _dockerClient.Images.BuildImageFromDockerfileAsync(
                new ImageBuildParameters { AuthConfigs = _authConfigs, Dockerfile = _dockerFileName, Tags = new string[] { _id! } }, 
                imageContents, _authConfigs.Values, null,  new ContainerBuildProgress(json => Task.CompletedTask));
        }

        public virtual async Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            if (_dockerClient == null || _dockerFileName == null)
            {
                throw new InvalidOperationException("Image not built");
            }

            var existingContainers = await _dockerClient!.Containers.ListContainersAsync(new ContainersListParameters { All = true });
            var existingContainer = existingContainers.FirstOrDefault(container => container.Names.Any(name => name.Trim('/', '\\', ' ').Equals(_id, StringComparison.InvariantCultureIgnoreCase)));
            if (existingContainer != null)
            {
                await _dockerClient.Containers.RemoveContainerAsync(existingContainer.ID, new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
            }
            var hostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>(),
                NetworkMode = "bridge"
            };
            var exposedPorts = new Dictionary<string, EmptyStruct>();
            foreach (var exposedPort in _exposedPorts.GroupBy(port => port.Value))
            {
                hostConfig.PortBindings[exposedPort.Key.ToString()] = exposedPort.Select(port => new PortBinding { HostPort = port.Key.ToString(), HostIP = "0.0.0.0" }).ToList();
                exposedPorts[exposedPort.First().Value.ToString()] = new EmptyStruct();
            }
            var createContainerResponse = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters { Image = _id, Name = _id, HostConfig = hostConfig, ExposedPorts = exposedPorts });
            _containerId = createContainerResponse.ID;
            await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
            var containerInspectResponse = await _dockerClient.Containers.InspectContainerAsync(_containerId);
            _addresses = new List<string>();
            _addresses.Add(containerInspectResponse.NetworkSettings.IPAddress);
            if (containerInspectResponse.NetworkSettings.SecondaryIPAddresses != null && containerInspectResponse.NetworkSettings.SecondaryIPAddresses.Any())
            {
                _addresses.AddRange(containerInspectResponse.NetworkSettings.SecondaryIPAddresses.Select(address => address.Addr));
            }
            if (containerInspectResponse.NetworkSettings.SecondaryIPv6Addresses != null && containerInspectResponse.NetworkSettings.SecondaryIPv6Addresses.Any())
            {
                _addresses.AddRange(containerInspectResponse.NetworkSettings.SecondaryIPv6Addresses.Select(address => address.Addr));
            }
            _addresses.Add("127.0.0.1");
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
            await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
            await _dockerClient.Images.DeleteImageAsync(_id, new ImageDeleteParameters { Force = true });
            _dockerClient.Dispose();
        }

        public void UseMessageChannel(IHostedInstanceMessageChannel messageChannel)
        {
            _messageChannel = messageChannel;
        }

        /// <summary>
        /// Configures the container image building process.
        /// </summary>
        /// <param name="configureBuildFunc"></param>
        public void OnConfigureBuild(Func<ContainerHostedInstance, IHostedInstanceContext, Task>? configureBuildFunc)
        {
            _configureBuildFunc = configureBuildFunc;
        }

        protected virtual async Task GenerateDockerFile(HostedInstanceData instanceData)
        {
            _dockerFileName = $"{_id}.DockerFile";
            var dockerFileEnvironmentVariablesSection = string.Join(Environment.NewLine, _environmentVariables.Select(environmentVariable => $"ENV {environmentVariable.Key}={environmentVariable.Value}")
                .Union(instanceData.ToEnvironmentVariables()).Select(envVar => $"ENV {envVar}"));
            var dockerFileDotNetArgument = typeof(Program).Assembly.GetName().Name;
            var dockerFileContent = string.Format(DockerFileFormat, _baseImage, dockerFileEnvironmentVariablesSection, dockerFileDotNetArgument);
            await File.WriteAllTextAsync(_dockerFileName, dockerFileContent);
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

                var fileDataBuffer = new byte[32 * 1024];
                while (true)
                {
                    var numRead = await fileStream.ReadAsync(fileDataBuffer, 0, fileDataBuffer.Length);
                    if (numRead <= 0)
                        break;

                    await tarArchiveOutputStream.WriteAsync(fileDataBuffer, 0, numRead);
                }

                await tarArchiveOutputStream.CloseEntryAsync(default);
            }
            await tarArchiveOutputStream.FlushAsync();
            tarArchiveOutputStream.Close();
            tarStream.Position = 0;

            return tarStream;
        }
    }
}
