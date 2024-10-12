using Microsoft.Extensions.Hosting;
using SandboxTest.Application;
using SandboxTest.Hosting;
using SandboxTest.Instance;
using SandboxTest.Sample.Application1;
using SandboxTest.Scenario;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite2
    {
        public readonly IInstance ApplicationHostedInstance1 = ApplicationHostedInstance.CreateEmptyInstance()
            .UseApplicationHostedInstanceMessageChannel()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });

        public readonly IInstance ApplicationHostedInstance2 = ApplicationHostedInstance.CreateEmptyInstance()
            .UseApplicationHostedInstanceMessageChannel()
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
