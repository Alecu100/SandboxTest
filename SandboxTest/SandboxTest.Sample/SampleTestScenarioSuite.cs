using Microsoft.Extensions.Hosting;
using SandboxTest.Application;
using SandboxTest.Hosting;
using SandboxTest.Instance;
using SandboxTest.Sample.Application1;
using SandboxTest.Scenario;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite
    {
        public readonly IInstance ApplicationInstance1 = ApplicationInstance.CreateEmptyInstance()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });

        public readonly IInstance ApplicationInstance2 = ApplicationInstance.CreateEmptyInstance()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });


        [Scenario]
        public void TestScenario()
        {

        }
    }
}
