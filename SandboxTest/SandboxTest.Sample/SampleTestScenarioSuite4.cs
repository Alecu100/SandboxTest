﻿using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using SandboxTest.AspNetCore;
using SandboxTest.Hosting;
using SandboxTest.Net.Http;
using SandboxTest.Sample.Application2;
using System.Net.Http.Json;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite4
    {
        public readonly IInstance _applicationInstance41 = ApplicationInstance.CreateEmptyInstance("Instance41")
            .UseWebRunner(args =>
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureWebApplicationBuilder();
                return Task.FromResult(builder);
            })
            .ConfigureWebRunner(builder =>
            {
                builder.Services.AddControllers().ConfigureApplicationPartManager(parts => parts.ApplicationParts.Add(new AssemblyPart(typeof(WebApplicationExtensions).Assembly)));
                return Task.CompletedTask;
            }, webApp =>
            {
                webApp.ConfigureWebApplication();
                return Task.CompletedTask;
            })
            .ConfigureWebRunnerUrl("https://localhost:5566")
            .AddHttpClientController("https://localhost:5566")
            .AddHostController();

        [Scenario]
        public void TestScenario6()
        {
            var firstStep = _applicationInstance41.AddStep().InvokeController<HostController>((controller, ctx) =>
            {
                var actionDescriptors = controller.Host.Services.GetRequiredService<IActionDescriptorCollectionProvider>();
                var controllersDescriptors = actionDescriptors.ActionDescriptors
                      .Items
                      .OfType<ControllerActionDescriptor>();
                controllersDescriptors.Should().NotBeEmpty();
                return Task.CompletedTask;
            });

            var secondStep = _applicationInstance41.AddStep().InvokeController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/WeatherForecast");
                httpRequest.Headers.Add(HeaderNames.Accept, "text/plain");
                httpRequest.Headers.Add(HeaderNames.UserAgent, "test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecasts = await httpResponse.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                weatherForecasts.Should().NotBeNull();
                ctx["weatherforecasts"] = weatherForecasts;
            });
            var thirdStep = _applicationInstance41.AddStep().AddPreviousStep(firstStep).InvokeController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/WeatherForecast");
                httpRequest.Headers.Add(HeaderNames.Accept, "text/plain");
                httpRequest.Headers.Add(HeaderNames.UserAgent, "test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecasts = await httpResponse.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                weatherForecasts.Should().NotBeNull();
                var previousWeatherForecasts = ctx["weatherforecasts"] as List<WeatherForecast>;
                previousWeatherForecasts.Should().NotBeNull();
                previousWeatherForecasts?.Should().NotIntersectWith(weatherForecasts);
            });
        }

        public class Message
        {
            public string? Name { get; set; }

            public string? Description { get; set; }
        }
    }
}
