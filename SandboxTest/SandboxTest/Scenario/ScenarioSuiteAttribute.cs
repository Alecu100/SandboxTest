namespace SandboxTest.Scenario
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScenarioSuiteAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}
