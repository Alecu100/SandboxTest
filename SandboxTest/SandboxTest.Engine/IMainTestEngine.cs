namespace SandboxTest.Engine
{
    public interface IMainTestEngine
    {
        /// <summary>
        /// Method that runs scenarios from a given assembly
        /// </summary>
        /// <param name="assemblyPath">The assembly to run the scenarios from</param>
        /// <param name="scenarioRunParameters">The scenarios from the assembly to run</param>
        /// <returns></returns>
        Task RunScenariosAsync(string assemblyPath, IEnumerable<ScenarioParameters> scenarioRunParameters);

        /// <summary>
        /// Sets a callback method to publish the details once a scenarion has been ran.
        /// </summary>
        /// <param name="onScenarioRanAsyncCallback">The callback to call when a scenarion finishes running</param>
        /// <returns></returns>
        void OnScenarionRan(Func<ScenarioRunResult, Task> onScenarioRanAsyncCallback);

        /// <summary>
        /// Scans an assembly for scenarios and returns a list of all the scenarios found.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        IEnumerable<ScenarioParameters> ScanForScenarios(string assemblyPath);
    }
}
