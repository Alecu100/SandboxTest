namespace SandboxTest.Scenario
{
    /// <summary>
    /// Used to store data shared across the execution of separate steps.
    /// All objects stored must be serializable.
    /// </summary>
    public class ScenarioData : Dictionary<string, object?>
    {
    }
}
