namespace SandboxTest
{
    public interface IHostedInstanceInitializer
    {
        /// <summary>
        /// Initializes the hosted instance with the given data required to initialize the host.
        /// </summary>
        /// <param name="hostedInstanceData"></param>
        /// <returns></returns>
        Task InitalizeAsync(HostedInstanceData hostedInstanceData);
    }
}
