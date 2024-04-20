using FluentAssertions;
using SandboxTest.Net.Http;
using SandboxTest.WireMock;
using System.Net.Http.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite3
    {
        public readonly IInstance _applicationInstance31 = ApplicationInstance.CreateEmptyInstance("Instance31")
            .UseWireMockRunner()
            .ConfigureWireMockRunner(6677, false, false)
            .AddWireMockController()
            .AddHttpClientController("http://localhost:6677");

        [Scenario]
        public void TestScenario5()
        {
            var firstStep = _applicationInstance31.AddStep().InvokeController<WireMockController>((controller, ctx) =>
            {
                var message = new Message { Name = "test_message", Description = "test_description" };
                controller.WireMockServer.Given(Request.Create().WithPath("/test")).RespondWith(Response.Create().WithBodyAsJson(message));
                ctx["message"] = message;
                return Task.CompletedTask;
            });
            var secondStep = _applicationInstance31.AddStep().AddPreviousStep(firstStep).InvokeController<HttpClientController>(async (controller, ctx) =>
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/test");
                var httpResponse = await controller.HttpClient.SendAsync(httpRequest);
                httpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                var httpResponseMessage = await httpResponse.Content.ReadFromJsonAsync<Message>();
                var ctxMessage = ctx["message"] as Message;
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
