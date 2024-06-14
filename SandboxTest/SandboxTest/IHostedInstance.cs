namespace SandboxTest
{
    /// <summary>
    /// Represents an instance that is hosted and runs separately from the scenario and scenario suite,
    /// receiving messages from message channel used for various commands.
    /// </summary>
    public interface IHostedInstance : IInstance
    {
        /// <summary>
        /// Gets the communication channel used to send messages from and to the application instance.
        /// </summary>
        IMessageChannel MessageChannel { get; }

        /// <summary>
        /// Starts the host for the instance to run it inside the host.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData);

        /// <summary>
        /// Stops the current host for the application instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task StopAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData);
    }
}
