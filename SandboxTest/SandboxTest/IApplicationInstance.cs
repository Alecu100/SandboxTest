namespace SandboxTest
{
    /// <summary>
    /// Represents an instance of a application that contains some defined steps, a runner and various assigned controllers
    /// </summary>
    public interface IApplicationInstance: IApplicationControllerContainer, IApplicationRunnerContainer, IScenarioStepContainer
    {
        string Id { get; }

        /// <summary>
        /// Gets the communication sink used to send message from and to the application instance.
        /// </summary>
        IApplicationMessageSink MessageSink { get; }

        /// <summary>
        /// Starts the current application instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task StartAsync();

        /// <summary>
        /// Stops the current application instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Task StopAsync();

        /// <summary>
        /// Resets the instance of the application so that it can be reused for another test scenario, clearing all the registered steps and calling any reset functionality configured for the runner.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}
