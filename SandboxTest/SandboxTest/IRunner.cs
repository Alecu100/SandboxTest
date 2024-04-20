namespace SandboxTest
{
    /// <summary>
    /// Base interface for all runners exposing the standard functionality that they should implement.
    /// Not all methods ar required to actually do something such as ConfigureRunSandboxingAsync
    /// </summary>
    public interface IRunner
    {
        /// <summary>
        /// Configures the required functionality in order to run the application instance decoupled from
        /// external dependencies such as external databases. This method is async for compatibility reasons.
        /// </summary>
        Task ConfigureBuildAsync();

        /// <summary>
        /// Builds the application instance without running it.
        /// </summary>
        Task BuildAsync();

        /// <summary>
        /// Configures any remaining functionality to run that can only the done after the application is built and before
        /// running it.
        /// </summary>
        /// <returns></returns>
        Task ConfigureRunAsync();

        /// <summary>
        /// Runs the application instance.
        /// </summary>
        /// <returns></returns>
        Task RunAsync();

        /// <summary>
        /// Stops the application instance.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Resets the current runner.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}
