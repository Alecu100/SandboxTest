namespace SandboxTest.Instance.AttachedMethod
{
    /// <summary>
    /// Specifies the kind of injected method.
    /// </summary>
    public enum AttachedMethodType
    {
        /// <summary>
        /// This represents an attached method by the runner to itself to execute it after or before the given target method.
        /// </summary>
        RunnerToRunner,

        /// <summary>
        /// This represents an attached method by a controller to the runner to execute it after or before the given target method from the runner.
        /// </summary>
        ControllerToRunner,

        /// <summary>
        /// This represents an attached method by a hosted instance to itself to execute it after or before the given target method.
        /// </summary>
        HostedInstanceToHostedInstance,

        /// <summary>
        /// This represents an attached method by the message channel to the hosted instance to execute it after or before the given target method from the instance.
        /// </summary>
        MessageChannelToHostedInstance,

        /// <summary>
        /// This represents an attached method by the message channel to the runner to execute it after or before the given target method from the runner.
        /// </summary>
        MessageChannelToRunner
    }
}
