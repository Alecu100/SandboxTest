namespace SandboxTest
{
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
