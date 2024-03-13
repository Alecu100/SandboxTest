namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class LoadScenarioOperation : Operation
    {
        /// <summary>
        /// Creates a new instance of <see cref="LoaderOptimization"/>
        /// </summary>
        public LoadScenarioOperation(string scenarioName) 
        {
            TypeName = nameof(LoadScenarioOperation);
            ScenarioMethodName = scenarioName;
        }

        /// <summary>
        /// The name of the scenarion to load for this application instance.
        /// </summary>
        public string ScenarioMethodName { get; set; }
    }
}
