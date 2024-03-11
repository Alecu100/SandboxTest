using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public interface IScenarioSuiteTestEngine
    {
        /// <summary>
        /// Loads a scenario suit to execute the scenarios from it
        /// </summary>
        /// <param name="scenarioSuiteType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LoadScenarioSuiteAsync(Type scenarioSuiteType, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the scenario methods
        /// </summary>
        /// <param name="scenarionMethods"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken cancellationToken);

        /// <summary>
        /// Sets a callback method to publish the details once a scenarion has been ran.
        /// </summary>
        /// <param name="onScenarioRanAsyncCallback">The callback to call when a scenarion finishes running</param>
        /// <returns></returns>
        void OnScenarionRan(Func<ScenarioRunResult, Task> onScenarioRanAsyncCallback);

        /// <summary>
        /// Sets a callback method to notify interested observers that a scenario has started to be ran.
        /// </summary>
        /// <param name="onScenarioRunningAsyncCallback"></param>
        void OnScenarioRunning(Func<Scenario, Task> onScenarioRunningAsyncCallback);

        /// <summary>
        /// Closes all the opened application instances.
        /// </summary>
        /// <returns></returns>
        Task CloseApplicationInstancesAsync();
    }
}
