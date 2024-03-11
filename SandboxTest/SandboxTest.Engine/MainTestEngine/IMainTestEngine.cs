namespace SandboxTest.Engine.MainTestEngine
{
    public interface IMainTestEngine
    {
        /// <summary>
        /// Method that runs scenarios from a given assembly.
        /// </summary>
        /// <param name="scenarios">The scenarios from the assembly to run</param>
        /// <param name="runContext">The scenarios from the assembly to run</param>
        /// <returns></returns>
        Task RunScenariosAsync(IEnumerable<Scenario> scenarios, IMainTestEngineRunContext runContext);

        /// <summary>
        /// Stops executing the currently running scenarios.
        /// </summary>
        /// <returns></returns>
        Task StopRunningScenariosAsync();

        /// <summary>
        /// Scans an assembly for scenarios and returns a list of all the scenarios found.
        /// </summary>
        /// <param name="assemblyPaths"></param>
        /// <param name="scanContext"></param>
        /// <returns></returns>
        Task ScanForScenariosAsync(IEnumerable<string> assemblyPaths, IMainTestEngineScanContext scanContext);
    }
}
