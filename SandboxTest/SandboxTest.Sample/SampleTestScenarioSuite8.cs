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
        public readonly IInstance _applicationInstance81 = ApplicationHostedInstance.CreateEmptyInstance("Instance81")
            .UseApplicationHostedInstanceMessageChannel()
            .UseWebApplicationRunner(args =>
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureWebApplicationBuilder();
                return Task.FromResult(builder);
            })
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
            .ConfigureWebApplicationRunnerUrl("https://localhost:5533")
            .AddRunnerController();

        public readonly IInstance _applicationInstance82 = ApplicationInstance.CreateEmptyInstance("Instance82")
            .UseNodeRunner("localhost", 8050)
            .ConfigureNodeRunnerWithVite(PathUtils.AppendToPath(PathUtils.LocateFolderPath("SandboxTest")!, "SandboxTest.Sample.Application5\\sandboxtest.sample.application5.client"))
            .AddPlaywrightController(PlaywrightControllerBrowserType.Chromium, headless: false, slowMod: 50);

        [Scenario]
        public void TestScenario8()
        {
            var firstStep = _applicationInstance82.AddStep().InvokeController<PlaywrightController>(async (controller, ctx) =>
            {
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
            });
            var secondStep = _applicationInstance81.AddStep(firstStep).InvokeController<RunnerController>(async (controller, ctx) =>
            {
                await controller.RunRunnerAsync();
            });
            var thirdStep = _applicationInstance82.AddStep(secondStep).InvokeController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeHiddenAsync();
            });
            var forthStep = _applicationInstance81.AddStep(thirdStep).InvokeController<RunnerController>(async (controller, ctx) =>
            {
                await controller.StopRunnerAsync();
            });
            var fithStep = _applicationInstance82.AddStep(forthStep).InvokeController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Loading... Please refresh once the ASP.NET backend has started")).ToBeVisibleAsync();
            });
        }
    }
}
