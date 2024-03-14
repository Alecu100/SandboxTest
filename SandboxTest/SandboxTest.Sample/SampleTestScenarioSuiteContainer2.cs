using Microsoft.Extensions.Hosting;
using SandboxTest.Runners.Host;
using SandboxTest.Sample.Application1;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuiteContainer2
    {
        private readonly IApplicationInstance _applicationInstance21 = 
            ApplicationInstance.CreateEmptyInstance("Instance21")
            .UseHostApplicationRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostApplicationController();


        [Scenario]
        public void TestScenarioMethod2()
        {

        }
    }
}
