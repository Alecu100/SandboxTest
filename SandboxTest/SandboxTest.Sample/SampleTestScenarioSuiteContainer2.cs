using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;
using SandboxTest.Runners.Host;
using SandboxTest.Sample.Application1;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuiteContainer2
    {
        public readonly IApplicationInstance _applicationInstance21 = 
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
            var firstStep = _applicationInstance21.AddStep().InvokeController<HostApplicationController>((controller, ctx) =>
            {
                ctx["FirstGuid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance21.AddStep().AddPreviousStep(firstStep).InvokeController<HostApplicationController>((controller, ctx) =>
            {
                var newGuid = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                newGuid.Should().NotBe(ctx["FirstGuid"]);
                return Task.CompletedTask;
            });
        }
    }
}
