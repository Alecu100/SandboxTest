using Microsoft.Extensions.DependencyInjection;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.AspNetCore;
using Microsoft.AspNetCore.Builder;
using SandboxTest.Sample.Application5.Server;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using SandboxTest.Application;
using SandboxTest.Node;
using SandboxTest.Utils;
using SandboxTest.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite8
    {
        public readonly IInstance ApplicationInstance81 = ApplicationHostedInstance.CreateEmptyInstance()
            .UseApplicationHostedInstanceMessageChannel()
            .UseWebApplicationRunner(() =>
            {
                var builder = WebApplication.CreateBuilder();
                builder.ConfigureWebApplicationBuilder();
                return Task.FromResult(builder);
            }, "https://localhost:5533")
            .ConfigureWebApplicationRunner(builder =>
            {
                builder.Services.AddControllers().ConfigureApplicationPartManager(parts => parts.ApplicationParts.Add(new AssemblyPart(typeof(WebApplicationExtensions).Assembly)));
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(
                        name: "test",
                        policy =>
                        {
                            policy.WithOrigins("https://localhost:5533//*", "https://localhost:5533")
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
            .DisableAutoRun()
            .AddRunnerController();

        public readonly IInstance ApplicationInstance82 = ApplicationInstance.CreateEmptyInstance()
            .UseNodeRunner("localhost", 8050)
            .ConfigureNodeRunnerWithVite(PathUtils.AppendToPath(PathUtils.LocateFolderPath("SandboxTest")!, "SandboxTest.Sample.Application5\\sandboxtest.sample.application5.client"))
            .AddPlaywrightController(PlaywrightControllerBrowserType.Chromium, headless: false, slowMod: 50, ignoreHttpsErrors: true);

        [Scenario]
        public void TestScenario8()
        {
            var firstStep = ApplicationInstance82.AddStep().UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
                await Task.Delay(3000);
            });
            var secondStep = ApplicationInstance81.AddStep(firstStep).UseController<RunnerController>(async (controller, ctx) =>
            {
                await controller.RunRunnerAsync();
            });
            var thirdStep = ApplicationInstance82.AddStep(secondStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeHiddenAsync();
                await Task.Delay(3000);
            });
            var forthStep = ApplicationInstance81.AddStep(thirdStep).UseController<RunnerController>(async (controller, ctx) =>
            {
                await controller.StopRunnerAsync();
            });
            var fithStep = ApplicationInstance82.AddStep(forthStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
                await Task.Delay(3000);
            });
        }
    }
}
