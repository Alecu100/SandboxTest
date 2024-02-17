using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using SandboxTest.Engine;
using System.Diagnostics;

namespace SandboxTest.TestAdapter
{
    [DefaultExecutorUri("executor://sandboxtest.testadapter")]
    public class SandboxTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            var mainTestEngine = new MainTestEngine();

            foreach (var source in sources) 
            {
                var scenarions = mainTestEngine.ScanForScenarios(source);
                foreach (var scenario in scenarions) 
                {
                    discoverySink.SendTestCase(ConvertScenarioParametersToTestCase(scenario, source));
                }
            }
        }


        private TestCase ConvertScenarioParametersToTestCase(ScenarioParameters scenarioParameters, string source) 
        {
            var testCase = new TestCase($"{scenarioParameters.ScenarioContainerFullyQualifiedName}.{scenarioParameters.ScenarioMethodName}", new Uri("executor://sandboxtest.testadapter"), source);
            return testCase;
        }
    }
}
