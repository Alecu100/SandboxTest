using SandboxTest.Instance.Hosted;
using System.Net.Sockets;

namespace SandboxTest.Container
{
    /// <summary>
    /// A container instance message channel that is using to communicate with container hosted instances.
    /// </summary>
    public class ContainerHostedInstanceMessageChannel : IHostedInstanceMessageChannel
    {
        private short _port;
        private Socket? _socket;
        private string _host;

        /// <summary>
        /// Creates a new instance of the <see cref="ContainerHostedInstanceMessageChannel"/> with the 
        /// specified port to connect to the container hosted instance using sockets on the given port.
        /// </summary>
        /// <param name="port"></param>
        public ContainerHostedInstanceMessageChannel(short port) 
        {
            _port = port;
        }

        public Task<string> ReceiveMessageAsync()
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(string applicationId, Guid runId, bool isInstance)
        {
            if (isInstance)
            {
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _socket.ConnectAsync(_host, _port);
            }
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
