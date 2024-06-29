using SandboxTest.Engine.Operations;

namespace SandboxTest.Engine.ChildTestEngine
{
    public interface IChildTestEngine
    {
        /// <summary>
        /// Gets the currently running application instace.
        /// </summary>
        IInstance? RunningInstance { get; }

        /// <summary>
        /// Runs an application instance
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="scenarioContainerFullyQualifiedName"></param>
        /// <param name="applicationInstanceId"></param>
        /// <returns></returns>
        Task<OperationResult> LoadInstanceAsync(string sourceAssemblyNameFulPath, string scenarioSuiteTypeFullName, string applicationInstanceId);

        /// <summary>
        /// Loads a specific scenario adding the configured steps to the current application instance.
        /// </summary>
        /// <param name="scenarioMethodName"></param>
        /// <returns></returns>
        Task<OperationResult> LoadScenarioAsync(string scenarioMethodName);

        /// <summary>
        /// Runs a step for the current application instance.
        /// </summary>
        /// <param name="stepId">The id of the step to run.</param>
        /// <param name="stepContext">The index of the step to run.</param>
        /// <returns></returns>
        Task<OperationResult> RunStepAsync(ScenarioStepId stepId, ScenarioStepData stepContext);

        /// <summary>
        /// Resets the current application instance preparing it to run another scenario.
        /// </summary>
        /// <returns></returns>
        Task<OperationResult> ResetInstanceAsync();

        /// <summary>
        /// Stops the runner.
        /// </summary>
        /// <returns></returns>
        Task<OperationResult> StopInstanceAsync();
        Task<OperationResult> RunInstanceAsync();
    }
}
