using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using SandboxTest.AspNetCore;
using SandboxTest.Net.Http;
using SandboxTest.Sample.Application2;
using System.Net.Http.Json;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite4
    {
        public readonly IApplicationInstance _applicationInstance41 = ApplicationInstance.CreateEmptyInstance("Instance41")
            .UseWebApplicationRunner(args =>
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureWebApplicationBuilder();
                return Task.FromResult(builder);
            })
            .ConfigureWebApplicationRunnerSandbox(builder =>
            {
                return Task.CompletedTask;
            },
            webApp =>
            {
                webApp.ConfigureWebApplication();
                return Task.CompletedTask;
            })
            .ConfigureWebApplicationRunnerUrl("http://localhost:5566")
            .AddHttpClientApplicationController("http://localhost:5566");

        [Scenario]
        public void TestScenario6()
        {
            var firstStep = _applicationInstance41.AddStep().InvokeController<HttpClientApplicationController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecast = await httpResponse.Content.ReadFromJsonAsync<WeatherForecast>();
                weatherForecast.Should().NotBeNull();
                ctx["weatherforecast"] = weatherForecast;
            });
            var secondStep = _applicationInstance41.AddStep().AddPreviousStep(firstStep).InvokeController<HttpClientApplicationController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecast = await httpResponse.Content.ReadFromJsonAsync<WeatherForecast>();
                weatherForecast.Should().NotBeNull();
                var previousWeatherForecast = ctx["weatherforecast"] as WeatherForecast;
                previousWeatherForecast.Should().NotBeNull();
                previousWeatherForecast?.Date.Should().NotBe(weatherForecast?.Date);
            });
        }

        public class Message
        {
            public string? Name { get; set; }

            public string? Description { get; set; }
        }
    }
}
