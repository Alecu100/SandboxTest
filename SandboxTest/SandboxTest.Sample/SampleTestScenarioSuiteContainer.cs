using Microsoft.Extensions.Hosting;
using SandboxTest.Runners.Host;
using SandboxTest.Sample.Application1;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuiteContainer
    {
        private readonly IApplicationInstance _applicationInstance1 =
            new ApplicationInstance("Instance1")
            .UseHostApplicationRunner(args =>
                {
                    var hostBuilder = Host.CreateDefaultBuilder(args);
                    hostBuilder.ConfigureHost();
                    return Task.FromResult(hostBuilder);
                });

        private readonly IApplicationInstance _applicationInstance2 =
            new ApplicationInstance("Instance2")
            .UseHostApplicationRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });


        [Scenario]
        public void TestScenarioMethod()
        {

        }
    }
}
