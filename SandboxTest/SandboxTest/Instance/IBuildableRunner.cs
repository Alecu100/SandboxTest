using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Represents a buildable runner, that executes additional methods before the main <see cref="IRunner.RunAsync"/> method.
    /// </summary>
    public interface IBuildableRunner : IRunner
    {
        /// <summary>
        /// Configures the required functionality in order to run the application instance decoupled from
        /// external dependencies such as external databases. This method is async for compatibility reasons.
        /// </summary>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -300)]
        Task ConfigureBuildAsync();

        /// <summary>
        /// Builds the application instance without running it.
        /// </summary>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -200)]
        Task BuildAsync();

        /// <summary>
        /// Configures any remaining functionality to run that can only the done after the application is built and before
        /// running it.
        /// </summary>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.RunnerToRunner, nameof(RunAsync), -100)]
        Task ConfigureRunAsync();
    }
}
