using SandboxTest.Engine.Operations;
using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.Engine.ChildTestEngine
{
    public interface IChildTestEngine
    {
        /// <summary>
        /// Gets the currently running application instace.
        /// </summary>
        IInstance? RunningInstance { get; }

        /// <summary>
        /// Gets the attached methods executor.
        /// </summary>
        IAttachedMethodsExecutor AttachedMethodsExecutor { get; }

        /// <summary>
        /// Gets the current scenario suite context.
        /// </summary>
        IScenarioSuiteContext? ScenarioSuiteContext { get; }

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
        /// <param name="scenarioSuiteData">The scenario suite data passed to the step to run.</param>
        /// <param name="stepData">The scenario data passed to the step to run.</param>
        /// <returns></returns>
        Task<OperationResult> RunStepAsync(ScenarioStepId stepId, ScenarioSuiteData scenarioSuiteData, ScenarioData stepData);

        /// <summary>
        /// Resets the current application instance preparing it to run another scenario.
        /// </summary>
        /// <returns></returns>
        Task<OperationResult> ResetInstanceAsync(ScenarioSuiteData scenarioSuiteData);

        /// <summary>
        /// Stops the instance by stopping the runner using <see cref="IRunner.StopAsync"/>.
        /// </summary>
        /// <returns></returns>
        Task<OperationResult> StopInstanceAsync(ScenarioSuiteData scenarioSuiteData);

        /// <summary>
        /// Runs the instance by stopping the runner using <see cref="IRunner.RunAsync"/>.
        /// </summary>
        /// <returns></returns>
        Task<OperationResult> RunInstanceAsync(ScenarioSuiteData scenarioSuiteData);
    }
}
