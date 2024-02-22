namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// 
    /// </summary>
    public class LoadScenarionOperation : Operation
    {
        /// <summary>
        /// Creates a new instance of <see cref="LoaderOptimization"/>
        /// </summary>
        public LoadScenarionOperation(string scenarioName) 
        {
            TypeName = nameof(LoadScenarionOperation);
            Name = scenarioName;
        }

        /// <summary>
        /// The name of the scenarion to load for this application instance.
        /// </summary>
        public string Name { get; set; }
    }
}
