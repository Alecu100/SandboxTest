using Docker.DotNet.Models;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

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
        private const string MessageSeparator = "e1ac64f9-6ddd-4437-a787-3a09c144d44e";

        private byte[] _socketBuffer = new byte[10000];

        private short _port;
        private Socket? _socket;
        private List<string>? _containerAddresses;

        /// <summary>
        /// The actual bytes of the Guid to separate messages and notify the end of a message.
        /// </summary>
        private static byte[] MessageSeparatorBytes;

        static ContainerHostedInstanceMessageChannel()
        {
            MessageSeparatorBytes = Guid.Parse(MessageSeparator).ToByteArray();
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
            int bytesRead;
            var offset = 0;
            do
            {
                bytesRead = await _socket!.ReceiveAsync(_socketBuffer.AsMemory(offset), SocketFlags.None);
                offset += bytesRead;
                if (_socketBuffer.Length <= offset + 1)
                {
                    var newSocketBuffer = new byte[_socketBuffer.Length + 5000];
                    _socketBuffer.CopyTo(newSocketBuffer, 0);
                    _socketBuffer = newSocketBuffer;
                }
                if (offset > MessageSeparatorBytes.Length)
                {
                    var index = 0;
                    while (index < MessageSeparatorBytes.Length && MessageSeparatorBytes[index] == _socketBuffer[offset - MessageSeparatorBytes.Length + index])
                    {
                        index++;
                    }
                    if (index == MessageSeparatorBytes.Length)
                    {
                        break;
                    }
                }
            }
            while (bytesRead > 0 || offset == 0);
            return Encoding.UTF8.GetString(_socketBuffer, 0, offset - MessageSeparatorBytes.Length);
        }

        /// <summary>
        /// Sends a message to the socket, blocking the caller until the message is sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendMessageAsync(string message)
        {
            if (_socket == null)
            {
                throw new InvalidOperationException("Container message channel not started");
            }
            var totalSent = 0;
            var sent = 0;
            var messageBytes = Encoding.UTF8.GetBytes(message).ToList();
            messageBytes.AddRange(MessageSeparatorBytes);
            var messageArray = messageBytes.ToArray();
            do
            {
                sent = await _socket.SendAsync(messageArray.AsMemory(totalSent), SocketFlags.None);
                totalSent += sent;
            }
            while (totalSent < messageArray.Length);
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

        public async Task OpenAsync(string applicationId, Guid runId, bool isInstance)
        {
            if (isInstance)
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                var cancellationTokenSource = new CancellationTokenSource();
                var acceptTasks = new List<Task<Socket>>();
                var sockets = new List<Socket>();

                foreach (var ipAddress in networkInterfaces.Select(address => address.GetIPProperties().UnicastAddresses.Select(address => address.Address)).SelectMany(x => x))
                {
                    var ipEndpoint = new IPEndPoint(ipAddress, _port);
                    var socket = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(ipEndpoint);
                    socket.Listen();
                    acceptTasks.Add(socket.AcceptAsync(cancellationTokenSource.Token).AsTask());
                    sockets.Add(socket);
                }

                _socket = await await Task.WhenAny(acceptTasks);
                cancellationTokenSource.Cancel();
                foreach (var socket in sockets)
                {
                    if (socket != _socket)
                    {
                        socket.Dispose();
                    }
                }
            }
            else
            {
                var connectTasks = new List<Task<Socket>>();
                var sockets = new List<Socket>();
                var cancellationTokenSource = new CancellationTokenSource();

                foreach (var ipAddress in _containerAddresses!)
                {
                    var ipEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), _port);
                    var socket = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    var connectFunc = async () =>
                    {
                        while (true)
                        {

                            try
                            {
                                await socket.ConnectAsync(ipEndpoint, cancellationTokenSource.Token);
                                return socket;
                            }
                            catch(SocketException)
                            {
                            }
                        }

                    };
                    connectTasks.Add(connectFunc());
                    sockets.Add(socket);
                }

                _socket = await await Task.WhenAny(connectTasks);
                cancellationTokenSource.Cancel();
                foreach (var socket in sockets) 
                {
                    if (socket != _socket)
                    {
                        socket.Dispose();
                    }
                }
            }
        }
    }
}
