using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest.Engine
{
    public class ChildTestEngine : IChildTestEngine
    {
        public Task<OperationResult> BuildScenarioAsync(string scenarioMethodName)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> ResetApplicationInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> RunApplicationInstanceAsync(string assemblyPath, string scenarioContainerFullyQualifiedName, string applicationInstanceId)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> RunStep(int stepIndex)
        {
            throw new NotImplementedException();
        }
    }
}
