namespace SandboxTest
{
    /// <summary>
    /// Represents an application controller used to execute actions on application instances and to control an application instance in general.
    /// </summary>
    public interface IController
    {
        string? Name { get; }

        /// <summary>
        /// Does any configuration related to the application instance before the application instance is built.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task ConfigureBuildAsync(IInstance applicationInstance);

        /// <summary>
        /// Builds the application controller for the specific application instance
        /// </summary>
        /// <returns></returns>
        Task BuildAsync(IInstance applicationInstance);

        /// <summary>
        /// Resets the current application controller to the reused for another scenario.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task ResetAsync(IInstance applicationInstance);
    }
}
