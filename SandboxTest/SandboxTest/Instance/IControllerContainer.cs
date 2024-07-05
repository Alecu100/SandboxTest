namespace SandboxTest.Instance
{
    /// <summary>
    /// Represents a container of application controllers.
    /// </summary>
    public interface IControllerContainer
    {
        /// <summary>
        /// Gets a list of all the controllers from the controller container.
        /// </summary>
        IReadOnlyList<IController> Controllers { get; }

        /// <summary>
        /// Adds a controller to the controller container.
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <param name="controller"></param>
        /// <returns></returns>
        IInstance AddController<TController>(TController controller) where TController : IController;
    }
}
