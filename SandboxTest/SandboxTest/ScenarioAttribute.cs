namespace SandboxTest
{
    /// <summary>
    /// Added on public methods on public class to denote that the method represents a scenario.
    /// The main responsibility of such a method is to setup the scenario steps in the right order using <see cref="IScenarioStepContainer.AddStep"/>.
    /// For each step, one or more controllers can be invoked to actually execute test logic for that given step using <see cref="ScenarioStep.InvokeController{TController}(Func{TController, ScenarioStepContext, Task}, string?)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScenarioAttribute : Attribute
    {
        /// <summary>
        /// The description of the scenario
        /// </summary>
        public string? Description { get; set; }
    }
}
