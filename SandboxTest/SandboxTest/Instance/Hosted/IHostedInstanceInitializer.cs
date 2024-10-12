namespace SandboxTest.Instance.Hosted
{
    public interface IHostedInstanceInitializer
    {
        /// <summary>
        /// Initializes the hosted instance with the given data required to start the host.
        /// </summary>
        /// <param name="hostedInstanceData"></param>
        /// <returns></returns>
        Task InitalizeAsync(HostedInstanceData hostedInstanceData);

        /// <summary>
        /// Waits for the instance to be stopped.
        /// </summary>
        /// <returns></returns>
        Task<int> WaitToFinishAsync();
    }
}
