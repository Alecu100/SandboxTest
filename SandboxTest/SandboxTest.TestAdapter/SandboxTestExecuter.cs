using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using SandboxTest.Engine.MainTestEngine;
using System.Diagnostics;

namespace SandboxTest.TestAdapter
{
    [ExtensionUri("executor://sandboxtest.testadapter")]
    public class SandboxTestExecuter : ITestExecutor
    {
        private IMainTestEngine? _mainTestEngine;

        public void Cancel()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            _mainTestEngine?.StopRunningScenariosAsync()?.Wait();
        }

        public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            if (tests == null)
            {
                return;
            }

            _mainTestEngine = new MainTestEngine();
            var scenarios = tests.Select(test => test.ConvertToScenario());
            var scenarioRunContext = new SandboxTestExecuterRunContext(runContext, frameworkHandle);
            _ = typeof(ScenarioAttribute);
            _mainTestEngine.RunScenariosAsync(scenarios, scenarioRunContext).Wait();
        }

        public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }
    }
}
