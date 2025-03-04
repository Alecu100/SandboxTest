using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Docker.DotNet.Models;

namespace SandboxTest.Container
{
    /// <summary>
    /// A container instance message channel that is using to communicate with container hosted instances.
    /// </summary>
    public class ContainerHostedInstanceMessageChannel : IHostedInstanceMessageChannel
    {
        /// <summary>
        /// One guid is enough to separate messages.
        /// </summary>
        private const string MessageSeparatorEnd = "e1ac64f9-6ddd-4437-a787-3a09c144d44e";
        private const string MessageSeparatorStart = "30d57f7b-3aa4-4b0a-89a7-1ec5b9c2937b";
        private const string HostSideMessage = "pong";
        private const string TestSideMessage = "ping";
        private const int BackOffMiliseconds = 5;
        private static byte[] HostSideMessageBytes;
        private static byte[] TestSideMessageBytes;

        /// <summary>
        /// The actual bytes of the Guid to separate messages and notify the end of a message.
        /// </summary>
        private static byte[] MessageSeparatorEndBytes;
        private static byte[] MessageSeparatorStartBytes;

        private byte[] _socketReceiveBuffer = new byte[10000];
        private short _port;
        private Socket? _hostSocketListening;
        private Socket? _socket;
        private List<string>? _containerAddresses;
        private bool _isInHost;
        private IPEndPoint? _hostIpEndpoint;


        static ContainerHostedInstanceMessageChannel()
        {
            MessageSeparatorEndBytes = Guid.Parse(MessageSeparatorEnd).ToByteArray();
            MessageSeparatorStartBytes = Guid.Parse(MessageSeparatorStart).ToByteArray();
            HostSideMessageBytes = Encoding.UTF8.GetBytes(HostSideMessage);
            TestSideMessageBytes = Encoding.UTF8.GetBytes(TestSideMessage);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ContainerHostedInstanceMessageChannel"/> with the 
        /// specified port to connect to the container hosted instance using sockets on the given port.
        /// </summary>
        /// <param name="port"></param>
        public ContainerHostedInstanceMessageChannel(short port) 
        {
            _port = port;
        }

        /// <summary>
        /// Receives messages from the socket blocking the caller until a new message is received.
        /// </summary>
        /// <returns>The messages received from the socket</returns>
        public async Task<string> ReceiveMessageAsync()
        {
            if (_isInHost)
            {
                return await ReceiveMessageInHost();
            }

            return await ReceiveMessageOutsideHost();
        }

        private async Task<string> ReceiveMessageOutsideHost()
        {
            if (_hostIpEndpoint == null || _socket == null)
            {
                throw new InvalidOperationException("Host ip endpoint not found");
            }
            while (true)
            {
                try
                {
                    var totalReceived = await Receive(_socket, _socketReceiveBuffer);
                    if (totalReceived > 0)
                    {
                        return Encoding.UTF8.GetString(_socketReceiveBuffer, 0, totalReceived);
                    }
                }
                catch (Exception)
                {
                    _socket.Dispose();
                    _socket = new Socket(_hostIpEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await _socket.ConnectAsync(_hostIpEndpoint);
                }
                await Task.Delay(BackOffMiliseconds);
            }
        }

        private async Task<string> ReceiveMessageInHost()
        {
            if (_socket == null || _hostSocketListening == null)
            {
                throw new InvalidOperationException("Container message channel not started");
            }
            while (true)
            {
                try
                {
                    var totalReceived = await Receive(_socket, _socketReceiveBuffer);
                    if (totalReceived > 0)
                    {
                        return Encoding.UTF8.GetString(_socketReceiveBuffer, 0, totalReceived);
                    }
                }
                catch (Exception)
                {
                    _socket.Dispose();
                    _socket = await _hostSocketListening.AcceptAsync();
                }
            }
        }

        /// <summary>
        /// Sends a message to the socket, blocking the caller until the message is sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            if (_isInHost)
            {
                await SendMessageInHost(messageBytes);
                return;
            }

            await SendMessageOutsideHost(messageBytes);
        }

        private async Task SendMessageOutsideHost(byte[] messageBytes)
        {
            if (_hostIpEndpoint == null || _socket == null)
            {
                throw new InvalidOperationException("Host ip endpoint not found");
            }
            while (true)
            {
                try
                {
                    await Send(_socket, messageBytes, messageBytes.Length);
                    break;
                }
                catch (Exception)
                {
                    _socket.Dispose();
                    _socket = new Socket(_hostIpEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await _socket.ConnectAsync(_hostIpEndpoint);
                }
                await Task.Delay(BackOffMiliseconds);
            }
        }

        private async Task SendMessageInHost(byte[] messageBytes)
        {
            if (_hostSocketListening == null || _socket == null)
            {
                throw new InvalidOperationException("Container message channel not started");
            }

            while (true)
            {
                try
                {
                    await Send(_socket, messageBytes, messageBytes.Length);
                    break;
                }
                catch (Exception)
                {
                    _socket?.Dispose();
                    _socket = await _hostSocketListening.AcceptAsync();
                }
                await Task.Delay(BackOffMiliseconds);
            }
            return;
        }

        /// <summary>
        /// Adds to the container hosted instance the port assigned to it as an exposed and published port.
        /// </summary>
        /// <param name="containerHostedInstance">The container to which to expose and publish the port</param>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.MessageChannelToHostedInstance, nameof(ContainerHostedInstance.ConfigureBuildAsync), -100)]
        public Task ConfigureBuildAsync(ContainerHostedInstance containerHostedInstance, IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            containerHostedInstance.ExposedPorts.Add(new KeyValuePair<short, short>(_port, _port));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the ip addresses in the test host after the container hosted instance has started and has the ip addresses assigned.
        /// </summary>
        /// <param name="containerHostedInstance">The container from which to take the ip addresses</param>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.MessageChannelToHostedInstance, nameof(ContainerHostedInstance.StartAsync), 100)]
        public Task StartAsync(ContainerHostedInstance containerHostedInstance, IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            _containerAddresses = new List<string>(containerHostedInstance.Addresses);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts the container message channel.
        /// </summary>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="runId">The run id, unique for each scenario suite run.</param>
        /// <param name="isInHost"></param>
        /// <returns></returns>
        public async Task OpenAsync(string instanceId, Guid runId, bool isInHost)
        {
            _isInHost = isInHost;
            var cancellationTokenSource = new CancellationTokenSource();
            var candidateTasks = new List<Task>();

            if (_isInHost)
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                var candidateIpAddresses = networkInterfaces.Select(address => address.GetIPProperties().UnicastAddresses.Select(address => address.Address)).SelectMany(x => x);

                foreach (var ipAddress in candidateIpAddresses)
                {
                    candidateTasks.Add(TryContainerLocalIpAddress(cancellationTokenSource, ipAddress));
                }
                await Task.WhenAll(candidateTasks);
                if (_hostSocketListening == null)
                {
                    throw new InvalidOperationException("Could not find listening ip and socket in host");
                }
                _socket = await _hostSocketListening.AcceptAsync();
            }
            else
            {
                var candidateIpAddresses = _containerAddresses!.Select(IPAddress.Parse);

                foreach (var ipAddress in candidateIpAddresses)
                {
                    candidateTasks.Add(TryExternalContainerIpAddress(cancellationTokenSource, ipAddress));
                }
                await Task.WhenAll(candidateTasks);
                if (_hostIpEndpoint == null)
                {
                    throw new InvalidOperationException("Could not find host ip endpoint outside host");
                }
                _socket = new Socket(_hostIpEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await _socket.ConnectAsync(_hostIpEndpoint);
            }
        }

        private async Task TryContainerLocalIpAddress(CancellationTokenSource cancellationTokenSource, IPAddress ipAddress)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var ipEndpoint = new IPEndPoint(ipAddress, _port);
                var socket = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Bind(ipEndpoint);
                    socket.Listen();
                    using var incomingSocket = await socket.AcceptAsync(cancellationTokenSource.Token);
                    if (await CheckHostSidePing(incomingSocket))
                    {
                        cancellationTokenSource.Cancel();
                        _hostSocketListening = socket;
                        break;
                    }
                    await Task.Delay(BackOffMiliseconds);
                }
                catch (Exception)
                {
                    socket.Dispose();
                }
            }
        }

        private async Task TryExternalContainerIpAddress(CancellationTokenSource cancellationTokenSource, IPAddress ipAddress)
        {
            var ipEndpoint = new IPEndPoint(ipAddress, _port);

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    using var socket = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(ipEndpoint, cancellationTokenSource.Token);
                    if (await CheckTestSidePing(socket))
                    {
                        cancellationTokenSource.Cancel();
                        _hostIpEndpoint = ipEndpoint;
                        return;
                    }
                }
                catch (Exception)
                {
                }
                await Task.Delay(BackOffMiliseconds);
            }
        }

        private async Task<bool> CheckHostSidePing(Socket socket, byte[]? buffer = null)
        {
            if (buffer == null)
            {
                buffer = new byte[1000];
            }

            try
            {
                var totalReceived = await Receive(socket, buffer);
                if (totalReceived == 0)
                {
                    return false;
                }
                var ping = Encoding.UTF8.GetString(buffer, 0, totalReceived);
                if (!ping.Equals(TestSideMessage, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
                await Send(socket, HostSideMessageBytes, HostSideMessageBytes.Length);
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }

        private async Task<bool> CheckTestSidePing(Socket socket, byte[]? buffer = null)
        {
            if (buffer == null)
            {
                buffer = new byte[1000];
            }

            try
            {
                await Send(socket, TestSideMessageBytes, TestSideMessageBytes.Length);
                var totalReceived = await Receive(socket, buffer);
                if (totalReceived == 0)
                {
                    return false;
                }
                var ping = Encoding.UTF8.GetString(buffer, 0, totalReceived);
                if (!ping.Equals(HostSideMessage, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<int> Receive(Socket socket, byte[] buffer)
        {
            var received = 0;
            var totalReceived = 0;
            var receivedMessageSeparatorStart = false;
            do
            {
                received = await socket.ReceiveAsync(buffer.AsMemory(totalReceived), SocketFlags.None);
                totalReceived += received;
                var index = 0;
                while (received > 0 && index < MessageSeparatorStartBytes.Length && buffer[index] == MessageSeparatorStartBytes[index])
                {
                    index++;
                }
                if (index == MessageSeparatorStartBytes.Length)
                {
                    receivedMessageSeparatorStart = true;
                }
                else
                {
                    totalReceived = 0;
                }
                if (totalReceived > MessageSeparatorEndBytes.Length)
                {
                    index = 0;
                    while (index < MessageSeparatorEndBytes.Length && MessageSeparatorEndBytes[index] == buffer[totalReceived - MessageSeparatorEndBytes.Length + index])
                    {
                        index++;
                    }
                    if (index == MessageSeparatorEndBytes.Length)
                    {
                        break;
                    }
                }
            }
            while (totalReceived < buffer.Length && receivedMessageSeparatorStart == false && received > 0);
            totalReceived = totalReceived - MessageSeparatorEndBytes.Length - MessageSeparatorStartBytes.Length;
            for (int i = 0; totalReceived > 0 && i < totalReceived + MessageSeparatorStartBytes.Length; i++)
            {
                buffer[i] = buffer[i + MessageSeparatorStartBytes.Length];
            }
            return totalReceived > 0 ? totalReceived : 0;
        }

        private async Task Send(Socket socket, byte[] buffer, int count)
        {
            var sent = 0;
            var totalSent = 0;
            var delimitedBufferList = new List<byte>(MessageSeparatorStartBytes);
            delimitedBufferList.AddRange(buffer.Take(count));
            delimitedBufferList.AddRange(MessageSeparatorEndBytes);
            var delimitedBuffer = delimitedBufferList.ToArray();
            do
            {
                sent = await socket.SendAsync(delimitedBuffer.AsMemory(totalSent), SocketFlags.None);
                totalSent += sent;
            }
            while (totalSent < count);
        }
    }
}
