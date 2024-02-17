namespace SandboxTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScenarioAttribute : Attribute
    {
        /// <summary>
        /// The description of the scenario
        /// </summary>
        public string? Description { get; set; }
    }
}
