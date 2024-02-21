namespace SandboxTest.Engine.ChildTestEngine
{
    public class ChildTestEngine : IChildTestEngine
    {
        public virtual Task<OperationResult> BuildScenarioAsync(string scenarioMethodName)
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> ResetApplicationInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> RunApplicationInstanceAsync(string assemblyPath, string scenarioContainerFullyQualifiedName, string applicationInstanceId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> RunStep(int stepIndex)
        {
            throw new NotImplementedException();
        }
    }
}
