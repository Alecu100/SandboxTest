﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SandboxTest.Hosting;
using SandboxTest.Sample.Application1;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite2
    {
        public readonly IInstance _applicationInstance21 = ApplicationInstance.CreateEmptyInstance("Instance21")
            .UseHostRunner(args =>
            {
                var hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureHost();
                return Task.FromResult(hostBuilder);
            })
            .AddHostController();


        [Scenario]
        public void TestScenario2()
        {
            var firstStep = _applicationInstance21.AddStep().InvokeController<HostController>((controller, ctx) =>
            {
                ctx["FirstGuid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance21.AddStep().AddPreviousStep(firstStep).InvokeController<HostController>((controller, ctx) =>
            {
                var newGuid = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                newGuid.GetNewGuid().Should().NotBe(ctx["FirstGuid"].ToString());
                return Task.CompletedTask;
            });
        }

        [Scenario]
        public void TestScenario3_Should_Fail()
        {
            var firstStep = _applicationInstance21.AddStep().InvokeController<HostController>((controller, ctx) =>
            {
                ctx["FirstGuid"] = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>().GetNewGuid();
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance21.AddStep().AddPreviousStep(firstStep).InvokeController<HostController>((controller, ctx) =>
            {
                var newGuid = controller.Host.Services.GetRequiredService<IRandomGuidGenerator>();
                newGuid.GetNewGuid().Should().Be(ctx["FirstGuid"].ToString());
                return Task.CompletedTask;
            });
        }
    }
}
