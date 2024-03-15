namespace SandboxTest
{
    /// <summary>
    /// Represents a container of application controllers.
    /// </summary>
    public interface IApplicationControllerContainer
    {
        /// <summary>
        /// Gets a list of all the controllers from the application controller container.
        /// </summary>
        IReadOnlyList<IApplicationController> Controllers { get; }

        /// <summary>
        /// Adds a controller to the application controller container.
        /// </summary>
        /// <typeparam name="TApplicationController"></typeparam>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ApplicationInstance AddController<TApplicationController>(TApplicationController controller, string? name = null) where TApplicationController : IApplicationController;
    }
}
