namespace SandboxTest.Instance.AttachedMethod
{
    /// <summary>
    /// Specifies the kind of injected method.
    /// </summary>
    public enum AttachedMethodType
    {
        /// <summary>
        /// This represents an injected step by the runner to itself to execute it after or before the given target method.
        /// </summary>
        RunnerToRunner,

        /// <summary>
        /// This represents an injected step by a controller to the runner to execute it after or before the given target method from the runner.
        /// </summary>
        ControllerToRunner
    }
}
