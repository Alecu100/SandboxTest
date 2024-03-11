using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using SandboxTest.Engine;
using SandboxTest.Engine.MainTestEngine;

namespace SandboxTest.TestAdapter
{
    public class SandboxTestDiscovererScanContext : IMainTestEngineScanContext
    {
        private IDiscoveryContext _discoveryContext;
        private IMessageLogger _logger;
        private ITestCaseDiscoverySink _discoverySink;

        public SandboxTestDiscovererScanContext(IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            _discoveryContext = discoveryContext;
            _logger = logger;
            _discoverySink = discoverySink;
        }

        /// <summary>
        /// Calls the test host to send the discovered scenario as a test case.
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public Task OnScenarioFound(Scenario scenario)
        {
            _logger.SendMessage(TestMessageLevel.Informational, "Sending found test case to the test runtime.");
            _discoverySink.SendTestCase(scenario.ConvertToTestCase());
            return Task.CompletedTask;
        }
    }
}
