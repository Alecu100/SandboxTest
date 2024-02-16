namespace SandboxTest
{
    public interface IApplicationController
    {
        string Name { get; }

        /// <summary>
        /// Does any configuration related to the application instance before the application instance is built.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task ConfigureBuildAsync(IApplicationInstance applicationInstance);

        /// <summary>
        /// Builds the application controller for the specific application instance
        /// </summary>
        /// <returns></returns>
        Task BuildAsync(IApplicationInstance applicationInstance);
    }
}
