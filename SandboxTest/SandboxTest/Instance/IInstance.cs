namespace SandboxTest.Instance
{
    /// <summary>
    /// Represents an instance of a application that contains some defined steps, a runner and various assigned controllers
    /// </summary>
    public interface IInstance : IControllerContainer, IRunnerContainer, IScenarioStepContainer
    {
        /// <summary>
        /// Gets the instance id used to identify it.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Initializes the hosted instance with the given id before actually using it. It can't be used without being initialized.
        /// </summary>
        /// <param name="id"></param>
        void Initialize(string id);

        /// <summary>
        /// Represents the startup order of the instance. To disable automatic startup, it must be set to null.
        /// </summary>
        int? Order { get; set; }

        /// <summary>
        /// Resets the instance so that it can be reused for another test scenario, clearing all the registered steps.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}
