using Newtonsoft.Json;
using System.IO.Pipes;
using System.Text;

namespace SandboxTest
{
    /// <summary>
    /// Represents the default pipe application message sink using the operating system pipes to send messages from and to application instances.
    /// </summary>
    public class PipeApplicationMessageSink : IApplicationMessageSink
    {
        /// <summary>
        /// Gets the name of a pipe used to communicate with child application hosts to send commands like executing a step.
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        private static string GetChildApplicationInstanceHostPipeName(Guid runId, string applicationInstanceId)
        {
            return $"application-instance-id-{applicationInstanceId}-run-id-{runId}";
        }

        /// <summary>
        /// The default settings for serializing json objects in the json pipes.
        /// </summary>
        private static readonly JsonSerializerSettings PipeJsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private Lazy<Task<NamedPipeClientStream>>? _instanceClientPipeStream;
        private Lazy<Task<NamedPipeServerStream>>? _instanceServerPipeStream;
        private byte[] _instancePipeStreamBuffer = new byte[10000];

        /// <summary>
        /// Reads a string message from the pipe stream that can be a json.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> ReceiveMessageAsync()
        {
            PipeStream? pipeStream = null;
            if (_instanceClientPipeStream != null)
            {
                pipeStream = await _instanceClientPipeStream.Value;
            }
            if (_instanceServerPipeStream != null)
            {
                pipeStream = await _instanceServerPipeStream.Value;
            }
            if (pipeStream == null)
            {
                throw new InvalidOperationException("Pipe application message sink not started");
            }

            int bytesRead;
            var offset = 0;
            do
            {
                bytesRead = await pipeStream.ReadAsync(_instancePipeStreamBuffer, offset, _instancePipeStreamBuffer.Length - offset);
                offset += bytesRead;
            }
            while (bytesRead > 0);
            return Encoding.UTF8.GetString(_instancePipeStreamBuffer, 0, offset);
        }

        /// <summary>
        /// Writes a string message to the pipe stream that can be a json.
        /// </summary>
        /// <param name="message">The string message.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendMessageAsync(string message)
        {
            PipeStream? pipeStream = null;
            if (_instanceClientPipeStream != null)
            {
                pipeStream = await _instanceClientPipeStream.Value;
            }
            if (_instanceServerPipeStream != null)
            {
                pipeStream = await _instanceServerPipeStream.Value;
            }
            if (pipeStream == null)
            {
                throw new InvalidOperationException("Pipe application message sink not started");
            }
 
            await pipeStream.WriteAsync(Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Only sets the client and server pipe streams but waits to starts them until the first message is sent.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="runId"></param>
        /// <param name="isApplicationInstance"></param>
        /// <returns></returns>
        public Task StartAsync(string applicationId, Guid runId, bool isApplicationInstance)
        {
            if (!isApplicationInstance)
            {
                _instanceClientPipeStream = new Lazy<Task<NamedPipeClientStream>>(async () =>
                {
                    var pipe = new NamedPipeClientStream(".", GetChildApplicationInstanceHostPipeName(runId, applicationId), PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                    await pipe.ConnectAsync();
                    return pipe;
                });
            }
            else
            {
                _instanceServerPipeStream = new Lazy<Task<NamedPipeServerStream>>(async () =>
                {
                    var pipe = new NamedPipeServerStream(GetChildApplicationInstanceHostPipeName(runId, applicationId), PipeDirection.InOut, 5, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                    await pipe.WaitForConnectionAsync();
                    return pipe;
                });
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Closes opened server or client pipe streams.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            if (_instanceClientPipeStream != null)
            {
                var clientPipeStream = await _instanceClientPipeStream.Value;
                clientPipeStream.Close();
            }
            if (_instanceServerPipeStream != null)
            {
                var serverClientPipeStream = await _instanceServerPipeStream.Value;
                serverClientPipeStream.Close();
            }
        }
    }
}
