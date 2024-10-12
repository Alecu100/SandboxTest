namespace SandboxTest.Instance.Hosted
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
        IHostedInstanceMessageChannel? MessageChannel { get; }

        /// <summary>
        /// Assigns a specific message channel to the hosted instance.
        /// </summary>
        /// <param name="messageChannel"></param>
        void UseMessageChannel(IHostedInstanceMessageChannel messageChannel);

        /// <summary>
        /// Starts the host for the instance to run it inside the host. It run in the test host.
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

        /// <summary>
        /// Gets whether it should package the hosted instance in a separate dedicated folder.
        /// </summary>
        bool IsPackaged { get; }    

        /// <summary>
        /// Gets the address of the host which can be an ip or dns address.
        /// </summary>
        IReadOnlyList<string> Addresses { get; }
    }
}
