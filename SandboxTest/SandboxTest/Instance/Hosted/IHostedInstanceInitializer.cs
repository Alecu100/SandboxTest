using SandboxTest.Loader;

namespace SandboxTest.Instance.Hosted
{
    public interface IHostedInstanceInitializer
    {
        /// <summary>
        /// Initializes the hosted instance with the given data required to initialize the host.
        /// </summary>
        /// <param name="scenarioAssemblyLoadContext">The same scenario assembly load context used to load the hosted instance initializer that must be always used to load further components used for the test.</param>
        /// <param name="hostedInstanceData">The data received from the test engine that is required to initialize the hosted instance by the test runtime.</param>
        /// <returns></returns>
        Task InitalizeAsync(ScenariosAssemblyLoadContext scenarioAssemblyLoadContext, HostedInstanceData hostedInstanceData);

        /// <summary>
        /// Waits for the instance to be stopped.
        /// </summary>
        /// <returns></returns>
        Task<int> WaitToFinishAsync();
    }
}
