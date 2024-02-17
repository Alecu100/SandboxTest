namespace SandboxTest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScenarioSuiteAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}
