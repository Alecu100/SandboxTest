namespace SandboxTest.Engine.ChildTestEngine
{
    /// <summary>
    /// Initializes all the scenario suite instances by generating unique ids for them.
    /// </summary>
    public interface IScenarioSuiteInitializer
    {
        void Initialize(object scenarioSuite);
    }
}
