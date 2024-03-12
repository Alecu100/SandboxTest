namespace SandboxTest
{
    /// <summary>
    /// Used to store data shared across the execution of separate steps.
    /// All objects stored must be serializable.
    /// </summary>
    public class ScenarioStepContext : Dictionary<string, object>
    {

    }
}
