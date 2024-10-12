using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public interface IScenarioSuiteTestEngine
    {
        /// <summary>
        /// Loads a scenario suit to execute the scenarios from it
        /// </summary>
        /// <param name="scenarioSuiteType"></param>
        /// <param name="mainTestEngineRunContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LoadScenarioSuiteAsync(Type scenarioSuiteType, IMainTestEngineRunContext mainTestEngineRunContext, CancellationToken cancellationToken);

        /// <summary>
        /// Runs the scenario methods
        /// </summary>
        /// <param name="scenarionMethods"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken cancellationToken);

        /// <summary>
        /// Closes all the opened application instances.
        /// </summary>
        /// <returns></returns>
        Task CloseInstancesAsync();
    }
}
