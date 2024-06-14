namespace SandboxTest
{
    /// <summary>
    /// Represents an instance of a application that contains some defined steps, a runner and various assigned controllers
    /// </summary>
    public interface IInstance: IControllerContainer, IRunnerContainer, IScenarioStepContainer
    {
        string Id { get; }

        /// <summary>
        /// Resets the instance of the application so that it can be reused for another test scenario, clearing all the registered steps.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}
