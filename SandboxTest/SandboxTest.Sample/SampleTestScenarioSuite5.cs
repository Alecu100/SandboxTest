﻿using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using SandboxTest.Application;
using SandboxTest.AspNetCore;
using SandboxTest.Hosting;
using SandboxTest.Instance;
using SandboxTest.Net.Http;
using SandboxTest.Sample.Application2;
using SandboxTest.Scenario;
using System.Net.Http.Json;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite5
    {
        public readonly IInstance _applicationInstance41 = ApplicationHostedInstance.CreateEmptyInstance("Instance51")
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
                return Task.CompletedTask;
            }, webApp =>
            {
                webApp.ConfigureWebApplication();
                return Task.CompletedTask;
            })
            .ConfigureWebApplicationRunnerUrl("https://localhost:5600")
            .AddHttpClientController()
            .AddHostController()
            .AddWebApplicationController();

        [Scenario]
        public void TestScenario6()
        {
            var firstStep = _applicationInstance41.AddStep().UseController<HostController>((controller, ctx) =>
            {
                var actionDescriptors = controller.Host.Services.GetRequiredService<IActionDescriptorCollectionProvider>();
                var controllersDescriptors = actionDescriptors.ActionDescriptors
                      .Items
                      .OfType<ControllerActionDescriptor>();
                controllersDescriptors.Should().NotBeEmpty();
                return Task.CompletedTask;
            });

            var secondStep = _applicationInstance41.AddStep().UseController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/WeatherForecast");
                httpRequest.Headers.Add(HeaderNames.Accept, "text/plain");
                httpRequest.Headers.Add(HeaderNames.UserAgent, "test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecasts = await httpResponse.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                weatherForecasts.Should().NotBeNull();
                ctx.ScenarioData["weatherforecasts"] = weatherForecasts;
            });
            var thirdStep = _applicationInstance41.AddStep().AddPreviousStep(firstStep).UseController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/WeatherForecast");
                httpRequest.Headers.Add(HeaderNames.Accept, "text/plain");
                httpRequest.Headers.Add(HeaderNames.UserAgent, "test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var weatherForecasts = await httpResponse.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                weatherForecasts.Should().NotBeNull();
                var previousWeatherForecasts = ctx.ScenarioData["weatherforecasts"] as List<WeatherForecast>;
                previousWeatherForecasts.Should().NotBeNull();
                previousWeatherForecasts?.Should().NotIntersectWith(weatherForecasts);
            });
            var fourthStep = _applicationInstance41.AddStep().AddPreviousStep(thirdStep).UseController<WebApplicationController>((controller, ctx) =>
            {
                controller.WebApplication.Urls.Should().NotBeNull();
                controller.WebApplication.Urls.Should().Contain("https://localhost:5600");
            });
        }

        public class Message
        {
            public string? Name { get; set; }

            public string? Description { get; set; }
        }
    }
}
