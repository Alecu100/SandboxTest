namespace SandboxTest
{
    /// <summary>
    /// Represents a container for an application runner.
    /// </summary>
    public interface IRunnerContainer
    {
        /// <summary>
        /// Gets the used runner.
        /// </summary>
        IRunner? Runner { get; }

        /// <summary>
        /// Sets the runner to use.
        /// </summary>
        /// <typeparam name="TRunner"></typeparam>
        /// <param name="applicationRunner"></param>
        /// <returns></returns>
        IInstance UseRunner<TRunner>(TRunner applicationRunner) where TRunner : IRunner;
    }
}
