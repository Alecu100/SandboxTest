namespace SandboxTest
{
    /// <summary>
    /// Represents an instance that is hosted and runs separately from the scenario and scenario suite,
    /// receiving messages from message channel used for various commands.
    /// </summary>
    public interface IHostedInstance : IInstance
    {
        /// <summary>
        /// Gets and sets the communication channel used to send messages from and to the application instance.
        /// </summary>
        IHostedInstanceMessageChannel? MessageChannel { get; set; }

        /// <summary>
        /// Starts the host for the instance to run it inside the host.
        /// </summary>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token);

        /// <summary>
        /// Stops the current host for the application instance.
        /// </summary>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <returns></returns>
        Task StopAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData);
    }
}
