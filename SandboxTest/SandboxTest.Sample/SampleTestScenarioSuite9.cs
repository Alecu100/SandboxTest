using SandboxTest.Container;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.AspNetCore;
using Microsoft.AspNetCore.Builder;
using SandboxTest.Sample.Application6;
using Microsoft.Extensions.DependencyInjection;
using SandboxTest.Net.Http;
using FluentAssertions;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite9
    {
        public readonly IInstance _applicationInstance91 = ContainerHostedInstance.CreateEmptyInstance("Instance91")
            .UseContainerHostedInstanceMessageChannel(7008)
            .UseWebApplicationRunner(() =>
            {
                var builder = WebApplication.CreateBuilder();
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
                            policy.AllowAnyOrigin()
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
            .ConfigureWebApplicationRunnerUrl("http://0.0.0.0:6633")
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
