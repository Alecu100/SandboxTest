namespace SandboxTest
{
    /// <summary>
    /// Interface used to establish the communication channel for each application instance to send commands to
    /// </summary>
    public interface IApplicationMessageSink
    {
        /// <summary>
        /// Starts the application message sink
        /// </summary>
        /// <param name="runId">A unique identifier that represents the current test run.</param>
        /// <param name="isApplicationInstance">Denotes if the sink is in the application instance or in the main scenario instance.</param>
        /// <returns></returns>
        Task ConfigureAsync(string applicationId, Guid runId, bool isApplicationInstance);

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
