using SandboxTest.Container;
using SandboxTest.Instance;
using SandboxTest.Scenario;
using SandboxTest.AspNetCore;
using Microsoft.AspNetCore.Builder;
using SandboxTest.Sample.Application6;
using Microsoft.Extensions.DependencyInjection;
using SandboxTest.Net.Http;
using FluentAssertions;
using SandboxTest.Application;
using SandboxTest.WebServer;
using SandboxTest.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite9
    {
        public readonly IInstance _applicationInstance91 = ContainerHostedInstance.CreateEmptyInstance()
            .UseContainerHostedInstanceMessageChannel(7008)
            .ConfigureContainerHostedInstance((instance, ctx) =>
            {
                instance.ExposedPorts.Add(new KeyValuePair<short, short>(6633, 6633));
                return Task.CompletedTask;
            })
            .UseWebApplicationRunner(() =>
            {
                var builder = WebApplication.CreateBuilder();
                builder.ConfigureWebApplicationBuilder();
                builder.Logging.AddConsole();
                return Task.FromResult(builder);
            })
            .ConfigureWebApplicationRunner(builder =>
            {
                builder.Services.AddHttpLogging(o => { });
                return Task.CompletedTask;
            }, webApp =>
            {
                webApp.Use(Middleware404);
                webApp.UseHttpLogging();
                webApp.ConfigureWebApplication();
                webApp.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
                    string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
                return Task.CompletedTask;
            })
            .ConfigureWebApplicationRunnerUrl("http://0.0.0.0:6633");

        public readonly IInstance _applicationInstance92 = ApplicationInstance.CreateEmptyInstance()
            .UseRemoteWebServerRunner()
            .ConfigureRemoteWebServerRunnerUrl("http://localhost:6633")
            .AddHttpClientController();

        [Scenario]
        public void TestScenario9()
        {
            var firstStep = _applicationInstance92.AddStep().UseController<HttpClientController>(async (controller, ctx) =>
            {
                await Task.Delay(600000);
            });
            var secondStep = _applicationInstance92.AddStep(firstStep).UseController<HttpClientController>(async (controller, ctx) =>
            {
                var response = await controller.HttpClient.GetAsync("");
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            });
        }

        private static async Task Middleware404(HttpContext ctx, Func<Task> next)
        {
            ctx.Request.Host = new HostString("127.0.0.1:6633");
            await next();
            if (ctx.Response.StatusCode == 404)
            {
                int a = 0;
                a++;
            }
        }
    }
}
