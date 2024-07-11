using FluentAssertions;
using SandboxTest.Application;
using SandboxTest.Instance;
using SandboxTest.Net.Http;
using SandboxTest.Scenario;
using SandboxTest.WireMock;
using System.Net.Http.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite4
    {
        public readonly IInstance _applicationInstance41 = ApplicationInstance.CreateEmptyInstance("Instance41")
            .UseWireMockRunner(6677, false, false)
            .AddWireMockController()
            .AddHttpClientController();

        public readonly IInstance _applicationInstance42 = ApplicationHostedInstance.CreateEmptyInstance("Instance42")
            .UseApplicationHostedInstanceMessageChannel()
            .UseWireMockRunner(6688, false, false)
            .AddWireMockController()
            .AddHttpClientController();

        [Scenario]
        public void TestScenario51()
        {
            var firstStep = _applicationInstance41.AddStep().InvokeController<WireMockController>((controller, ctx) =>
            {
                var message = new Message { Name = "test_message", Description = "test_description" };
                controller.WireMockServer.Given(Request.Create().WithPath("/test")).RespondWith(Response.Create().WithBodyAsJson(message));
                ctx.ScenarioData["message"] = message;
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance41.AddStep().AddPreviousStep(firstStep).InvokeController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var httpResponseMessage = await httpResponse.Content.ReadFromJsonAsync<Message>();
                var ctxMessage = ctx.ScenarioData["message"] as Message;
                httpResponseMessage.Should().NotBeNull();
                httpResponseMessage?.Name.Should().Be(ctxMessage?.Name);
                httpResponseMessage?.Description.Should().Be(ctxMessage?.Description);
            });
        }

        [Scenario]
        public void TestScenario52()
        {
            var firstStep = _applicationInstance42.AddStep().InvokeController<WireMockController>((controller, ctx) =>
            {
                var message = new Message { Name = "test_message", Description = "test_description" };
                controller.WireMockServer.Given(Request.Create().WithPath("/test")).RespondWith(Response.Create().WithBodyAsJson(message));
                ctx.ScenarioData["message"] = message;
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance42.AddStep().AddPreviousStep(firstStep).InvokeController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var httpResponseMessage = await httpResponse.Content.ReadFromJsonAsync<Message>();
                var ctxMessage = ctx.ScenarioData["message"] as Message;
                httpResponseMessage.Should().NotBeNull();
                httpResponseMessage?.Name.Should().Be(ctxMessage?.Name);
                httpResponseMessage?.Description.Should().Be(ctxMessage?.Description);
            });
        }

        public class Message
        {
            public string? Name { get; set; }

            public string? Description { get; set; }
        }
    }
}
