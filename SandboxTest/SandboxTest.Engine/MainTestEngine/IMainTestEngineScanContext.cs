namespace SandboxTest.Engine.MainTestEngine
{
    public interface IMainTestEngineScanContext
    {
        /// <summary>
        /// Called when a scenario is found.
        /// </summary>
        /// <param name="scenario">The found scenarion</param>
        /// <returns></returns>
        Task OnScenarioFound(Scenario scenario);
    }
}
