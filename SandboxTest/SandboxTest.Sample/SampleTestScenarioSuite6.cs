using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Application;
using SandboxTest.Hosting;
using SandboxTest.Hosting.ServiceInterceptor;
using SandboxTest.Instance;
using SandboxTest.Sample.Application3;
using SandboxTest.Scenario;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite6
    {
        public readonly IInstance _applicationInstance51 = ApplicationInstance.CreateEmptyInstance("Instance61")
            .UseHostRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostController()
            .AddServiceIncerpeptorController();


        [Scenario]
        public void TestScenario7()
        {
            var firstStep = _applicationInstance51.AddStep().InvokeController<HostController>(async (controller, ctx) =>
            {
                await Task.Delay(1000);
            });
            var secondStep = _applicationInstance51.AddStep().AddPreviousStep(firstStep).InvokeController<HostController>(async (controller, ctx) =>
            {
                ctx.ScenarioData["guid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                await Task.Delay(1000);
            });
            var thirdStep = _applicationInstance51.AddStep().AddPreviousStep(secondStep).InvokeController<ServiceInterceptorController>((controller, ctx) =>
            {
                controller.UseInterceptor<IRandomGuidGenerator>().Intercept<Guid>(x => x.GetNewGuid).RecordsCall().ReturnsValue(Guid.Empty);
                return Task.CompletedTask;
            });
            var forthStep = _applicationInstance51.AddStep().AddPreviousStep(thirdStep).InvokeController<HostController>(async (controller, ctx) =>
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
