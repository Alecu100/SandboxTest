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
using SandboxTest.Net.Http;
using FluentAssertions;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite9
    {
        public readonly IInstance _applicationInstance91 = ContainerHostedInstance.CreateEmptyInstance("Instance91")
            .UseContainerHostedInstanceMessageChannel(7008)
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
            .AddHttpClientController();

        [Scenario]
        public void TestScenario9()
        {
            var firstStep = _applicationInstance91.AddStep().UseController<HttpClientController>(async (controller, ctx) =>
            {
                var response = await controller.HttpClient.GetAsync("");
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            });
        }
    }
}
