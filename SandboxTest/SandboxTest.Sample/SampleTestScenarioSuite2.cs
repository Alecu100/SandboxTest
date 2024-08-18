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
        private readonly IInstance _applicationHostedInstance1 = ApplicationHostedInstance.CreateEmptyInstance("Instance1")
            .UseApplicationHostedInstanceMessageChannel()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            });

        private readonly IInstance _applicationHostedInstance2 = ApplicationHostedInstance.CreateEmptyInstance("Instance2")
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
