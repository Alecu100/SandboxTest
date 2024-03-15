namespace SandboxTest
{
    /// <summary>
    /// Represents a container for an application runner.
    /// </summary>
    public interface IApplicationRunnerContainer
    {
        /// <summary>
        /// Gets the used runner.
        /// </summary>
        IApplicationRunner? Runner { get; }

        /// <summary>
        /// Sets the runner to use.
        /// </summary>
        /// <typeparam name="TRunner"></typeparam>
        /// <param name="applicationRunner"></param>
        /// <returns></returns>
        ApplicationInstance UseRunner<TRunner>(TRunner applicationRunner) where TRunner : IApplicationRunner;
    }
}
