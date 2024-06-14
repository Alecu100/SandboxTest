namespace SandboxTest
{
    /// <summary>
    /// Represents required data that the hosted instance needs to handle and pass to the 
    /// </summary>
    public class HostedInstanceData
    {
        required public Guid RunId { get; set; }

        required public string MainPath { get;set; }

        required public string AssemblySourceName { get; set; }

        required public string ScenarioSuiteTypeFullName { get; set; }

        required public string ApplicationInstanceId { get; set; }

        required public string HostedInstanceInitializerFullName {  get; set; }
    }
}
