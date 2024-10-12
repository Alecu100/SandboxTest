using Microsoft.Extensions.DependencyInjection;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.AspNetCore;
using Microsoft.AspNetCore.Builder;
using SandboxTest.Sample.Application4.Server;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using SandboxTest.Application;
using SandboxTest.Node;
using SandboxTest.Utils;
using SandboxTest.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite7
    {
        public readonly IInstance _applicationInstance71 = ApplicationHostedInstance.CreateEmptyInstance("Instance71")
            .UseApplicationHostedInstanceMessageChannel()
            .UseWebApplicationRunner(() =>
            {
                var builder = WebApplication.CreateBuilder();
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
                            policy.WithOrigins("https://localhost:5566//*", "https://localhost:5566")
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
            .ConfigureWebApplicationRunnerUrl("https://localhost:5566")
            .AddRunnerController();

        public readonly IInstance _applicationInstance72 = ApplicationInstance.CreateEmptyInstance("Instance72")
            .UseNodeRunner("localhost", 8088)
            .ConfigureNodeRunnerWithVite(PathUtils.AppendToPath(PathUtils.LocateFolderPath("SandboxTest")!, "sandboxtest.sample.application4.client"))
            .AddPlaywrightController(PlaywrightControllerBrowserType.Chromium, headless: false, slowMod: 50);

        [Scenario]
        public void TestScenario7()
        {
            var firstStep = _applicationInstance72.AddStep().UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await Expect(controller.Page.GetByText("This component demonstrates fetching data from the server")).ToBeVisibleAsync();
            });
            var secondStep = _applicationInstance72.AddStep().AddPreviousStep(firstStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await Task.Delay(7000);
            });
            var thirdStep = _applicationInstance72.AddStep().AddPreviousStep(secondStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await controller.Page.ReloadAsync();
                await Expect(controller.Page.GetByText("Weather forecast")).ToBeVisibleAsync();
            });
            var forthStep = _applicationInstance72.AddStep().AddPreviousStep(thirdStep).UseController<PlaywrightController>(async (controller, ctx) =>
            {
                await Task.Delay(5000);
            });
        }
    }
}
