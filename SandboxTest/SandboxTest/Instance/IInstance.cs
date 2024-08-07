﻿namespace SandboxTest.Instance
{
    /// <summary>
    /// Represents an instance of a application that contains some defined steps, a runner and various assigned controllers
    /// </summary>
    public interface IInstance : IControllerContainer, IRunnerContainer, IScenarioStepContainer
    {
        string Id { get; }

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
