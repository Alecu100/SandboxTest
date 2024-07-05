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

namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuite7
    {
        public readonly IInstance _applicationInstance71 = ApplicationHostedInstance.CreateEmptyInstance("Instance71")
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
                return Task.CompletedTask;
            }, webApp =>
            {
                webApp.ConfigureWebApplication();
                return Task.CompletedTask;
            })
            .ConfigureWebApplicationRunnerUrl("https://localhost:5566")
            .AddWebApplicationController();

        public readonly IInstance _applicationInstance72 = ApplicationInstance.CreateEmptyInstance("Instance72")
            .UseNodeRunner("localhost", 8088, false)
            .ConfigureNodeRunnerWithVite(PathUtils.AppendToPath(PathUtils.LocateFolderPath("SandboxTest")!, "sandboxtest.sample.application4.client"));


        [Scenario]
        public void TestScenario7()
        {
        }
    }
}
