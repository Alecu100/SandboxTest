using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Application;
using SandboxTest.Hosting;
using SandboxTest.Instance;
using SandboxTest.Sample.Application1;
using SandboxTest.Scenario;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite3
    {
        public readonly IInstance ApplicationInstance21 = ApplicationInstance.CreateEmptyInstance()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostController();


        [Scenario]
        public void TestScenario2()
        {
            var firstStep = ApplicationInstance21.AddStep().UseController<HostController>((controller, ctx) =>
            {
                ctx.ScenarioData["FirstGuid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
            });
            var secondStep = ApplicationInstance21.AddStep().AddPreviousStep(firstStep).UseController<HostController>((controller, ctx) =>
            {
                var newGuid = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                newGuid.GetNewGuid().Should().NotBe(ctx.ScenarioData["FirstGuid"]!.ToString());
            });
        }

        [Scenario]
        public void TestScenario3_Should_Fail()
        {
            var firstStep = ApplicationInstance21.AddStep().UseController<HostController>((controller, ctx) =>
            {
                ctx.ScenarioData["FirstGuid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
            });
            var secondStep = ApplicationInstance21.AddStep().AddPreviousStep(firstStep).UseController<HostController>((controller, ctx) =>
            {
                var newGuid = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                newGuid.GetNewGuid().Should().Be(ctx.ScenarioData["FirstGuid"]!.ToString());
            });
        }
    }
}
