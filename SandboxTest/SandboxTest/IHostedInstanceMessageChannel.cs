namespace SandboxTest
{
    /// <summary>
    /// Interface used to establish a communication channel for each application instance to receives various messages.
    /// This is used mostly to send commands to application instances to load a scenario, run a step or reset for a given scenario suite. 
    /// The default behavior must be blocking, so if no message is available it should block and wait until a message which contains a command is received.
    /// </summary>
    public interface IHostedInstanceMessageChannel
    {
        /// <summary>
        /// Starts the message channel before sending messages
        /// </summary>
        /// <param name="runId">A unique identifier that represents the current test run.</param>
        /// <param name="isInstance">Denotes on which side of the communication the channel is, on the instance side or on the test side.</param>
        /// <returns></returns>
        Task StartAsync(string applicationId, Guid runId, bool isInstance);

        /// <summary>
        /// Stops the application message sink
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Sends a message to the application instance.
        /// </summary>
        /// <returns></returns>
        Task SendMessageAsync(string message);

        /// <summary>
        /// Receives a message from the main scenario instance.
        /// </summary>
        /// <returns></returns>
        Task<string> ReceiveMessageAsync();
    }
}
