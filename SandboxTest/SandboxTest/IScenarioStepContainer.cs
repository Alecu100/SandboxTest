namespace SandboxTest
{
    /// <summary>
    /// Interface that denotes a container of steps, enabling operations such as adding a step or getting the added steps.
    /// </summary>
    public interface IScenarioStepContainer
    {
        /// <summary>
        /// Gets a list of all the added steps for a scenario step container.
        /// </summary>
        IReadOnlyList<ScenarioStep> Steps { get; }

        /// <summary>
        /// Gets a number representing the index after the last step's index.
        /// </summary>
        int CurrentStepIndex { get; }

        /// <summary>
        /// Adds an unnamed step.
        /// </summary>
        /// <returns></returns>
        ScenarioStep AddStep();

        /// <summary>
        /// Adds a more explicit named step.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ScenarioStep AddStep(string name);
    }
}
