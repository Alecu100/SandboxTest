using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Application;
using SandboxTest.Hosting;
using SandboxTest.Instance;
using SandboxTest.Sample.Application3;
using SandboxTest.Scenario;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite6
    {
        public readonly IInstance ApplicationInstance51 = ApplicationInstance.CreateEmptyInstance()
            .UseHostRunner(() =>
            {
                var hostBuilder = Host.CreateDefaultBuilder();
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostController()
            .AddServiceIncerpeptorController();


        [Scenario]
        public void TestScenario7()
        {
            var firstStep = ApplicationInstance51.AddStep().UseController<HostController>(async (controller, ctx) =>
            {
                await Task.Delay(1000);
            });
            var secondStep = ApplicationInstance51.AddStep().AddPreviousStep(firstStep).UseController<HostController>(async (controller, ctx) =>
            {
                ctx.ScenarioData["guid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                await Task.Delay(1000);
            });
            var thirdStep = ApplicationInstance51.AddStep().AddPreviousStep(secondStep).UseController<ServiceInterceptorController>((controller, ctx) =>
            {
                controller.UseInterceptor<IRandomGuidGenerator>().Intercept<Guid>(x => x.GetNewGuid).RecordsAllCalls().ReturnsValue(Guid.Empty);
                return Task.CompletedTask;
            });
            var forthStep = ApplicationInstance51.AddStep().AddPreviousStep(thirdStep).UseController<HostController>(async (controller, ctx) =>
            {
                var guidGenerator = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                var guid = guidGenerator.GetNewGuid();
                guid.Should().NotBe(Guid.Parse(ctx.ScenarioData["guid"]?.ToString() ?? throw new Exception("Original guid not found")));
                guid.Should().Be(Guid.Empty);
                await Task.Delay(6000);
            });
        }
    }
}
