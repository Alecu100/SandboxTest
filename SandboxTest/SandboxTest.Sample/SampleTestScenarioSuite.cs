using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;
using SandboxTest.Sample.Application1;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite
    {
        private readonly IInstance _applicationInstance1 = ApplicationInstance.CreateEmptyInstance("Instance1")
            .UseHostRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });

        private readonly IInstance _applicationInstance2 = ApplicationInstance.CreateEmptyInstance("Instance2")
            .UseHostRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });


        [Scenario]
        public void TestScenario()
        {

        }
    }
}
