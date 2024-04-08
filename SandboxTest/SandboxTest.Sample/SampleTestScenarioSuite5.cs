using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;
using SandboxTest.Hosting.ServiceInterceptor;
using SandboxTest.Sample.Application3;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite5
    {
        public readonly IApplicationInstance _applicationInstance51 = ApplicationInstance.CreateEmptyInstance("Instance51")
            .UseHostApplicationRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostApplicationController()
            .AddServiceIncerpeptorController();


        [Scenario]
        public void TestScenario7()
        {
            var firstStep = _applicationInstance51.AddStep().InvokeController<HostApplicationController>(async (controller, ctx) =>
            {
                await Task.Delay(1000);
            });
            var secondStep = _applicationInstance51.AddStep().AddPreviousStep(firstStep).InvokeController<HostApplicationController>(async (controller, ctx) =>
            {
                ctx["guid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                await Task.Delay(1000);
            });
            var thirdStep = _applicationInstance51.AddStep().AddPreviousStep(secondStep).InvokeController<ServiceInterceptorController>((controller, ctx) =>
            {
                controller.UseInterceptor<IRandomGuidGenerator>().Intercept<Guid>(x =>x.GetNewGuid).RecordsCall().ReturnsValue(Guid.Empty);
                return Task.CompletedTask;
            });
            var forthStep = _applicationInstance51.AddStep().AddPreviousStep(thirdStep).InvokeController<HostApplicationController>(async (controller, ctx) =>
            {
                var guidGenerator = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                var guid = guidGenerator.GetNewGuid();
                guid.Should().NotBe(Guid.Parse(ctx["guid"]?.ToString() ?? throw new Exception("Original guid not found")));
                guid.Should().Be(Guid.Empty);
                await Task.Delay(6000);
            });
        }
    }
}
