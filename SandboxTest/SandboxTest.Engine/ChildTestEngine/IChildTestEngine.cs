namespace SandboxTest.Engine
{
    public interface IChildTestEngine
    {
        Task<OperationResult> RunApplicationInstanceAsync(string assemblyPath, string scenarioContainerFullyQualifiedName, string applicationInstanceId);

        Task<OperationResult> BuildScenarioAsync(string scenarioMethodName);

        Task<OperationResult> RunStep(int stepIndex);

        Task<OperationResult> ResetApplicationInstanceAsync();
    }
}
