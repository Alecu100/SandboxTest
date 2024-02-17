using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest.Engine
{
    public class MainTestEngine : IMainTestEngine
    {
        public void OnScenarionRan(Func<ScenarioRunResult, Task> onScenarioRanAsyncCallback)
        {
            throw new NotImplementedException();
        }

        public Task RunScenariosAsync(string assemblyPath, IEnumerable<ScenarioParameters> scenarioRunParameters)
        {
            throw new NotImplementedException();
        }
    }
}
