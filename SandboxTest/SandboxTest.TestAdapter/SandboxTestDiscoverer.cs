using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using SandboxTest.Engine.MainTestEngine;
using SandboxTest.Scenario;
using System.Diagnostics;

namespace SandboxTest.TestAdapter
{
    [DefaultExecutorUri("executor://sandboxtest.testadapter")]
    public class SandboxTestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            var mainTestEngine = new MainTestEngine();
            var scenarioScanContext = new SandboxTestDiscovererScanContext(discoveryContext, logger, discoverySink);
            _ = typeof(ScenarioAttribute);
            mainTestEngine.ScanForScenariosAsync(sources, scenarioScanContext).Wait();
        }
    }
}
