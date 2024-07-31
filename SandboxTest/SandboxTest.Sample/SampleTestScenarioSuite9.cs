using SandboxTest.Container;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.Playwright;
using SandboxTest.AspNetCore;
using SandboxTest.Utils;
using Microsoft.AspNetCore.Builder;
using SandboxTest.Sample.Application6;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.Playwright.Assertions;
using System.Diagnostics;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite9
    {
        public readonly IInstance _applicationInstance91 = ContainerHostedInstance.CreateEmptyInstance("Instance91")
            .UseContainerHostedInstanceMessageChannel()
            .UseWebApplicationRunner(args =>
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureWebApplicationBuilder();
                return Task.FromResult(builder);
            })
            .ConfigureWebApplicationRunner(builder =>
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(
                        name: "test",
                        policy =>
                        {
                            policy.WithOrigins("https://localhost:6633//*", "https://localhost:6633")
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials()
                                  .SetIsOriginAllowed((x) => true);
                        });
                });
                return Task.CompletedTask;
            }, webApp =>
            {
                webApp.ConfigureWebApplication();
                webApp.UseCors("test");
                return Task.CompletedTask;
            })
            .ConfigureWebApplicationRunnerUrl("https://localhost:6633")
            .AddPlaywrightController(PlaywrightControllerBrowserType.Chromium, headless: false, slowMod: 50);

        [Scenario]
        public void TestScenario9()
        {
            var firstStep = _applicationInstance91.AddStep().UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
                await Task.Delay(3000);
            });
            var secondStep = _applicationInstance91.AddStep(firstStep).UseController<RunnerController>(async (controller, ctx) =>
            {
                await controller.RunRunnerAsync();
            });
            var thirdStep = _applicationInstance91.AddStep(secondStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeHiddenAsync();
                await Task.Delay(3000);
            });
            var forthStep = _applicationInstance91.AddStep(thirdStep).UseController<RunnerController>(async (controller, ctx) =>
            {
                await controller.StopRunnerAsync();
            });
            var fithStep = _applicationInstance91.AddStep(forthStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
                await Task.Delay(3000);
            });
        }
    }
}
