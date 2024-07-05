using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Base interface for all runners exposing the standard functionality that they should implement.
    /// Not all methods ar required to actually do something such as ConfigureRunSandboxingAsync
    /// </summary>
    public interface IRunner : IAttachedMethodContainer
    {
        /// <summary>
        /// Runs the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task RunAsync();

        /// <summary>
        /// Stops the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Resets the current runner.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        Task ResetAsync();
    }
}
