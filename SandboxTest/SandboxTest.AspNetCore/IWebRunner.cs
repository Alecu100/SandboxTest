using Microsoft.AspNetCore.Builder;
using SandboxTest.Hosting;
using SandboxTest.WebServer;

namespace SandboxTest.AspNetCore
{
    /// <summary>
    /// Represents a runner that exposes a WebApplication and a WebApplicationBuilder properties to be used by various controllers.
    /// </summary>
    public interface IWebRunner : IHostRunner, IWebServerRunner
    {
        /// <summary>
        /// Returns the web application.
        /// </summary>
        WebApplication WebApplication { get; }

        /// <summary>
        /// Returns the web application builder.
        /// </summary>
        WebApplicationBuilder WebApplicationBuilder { get; }
    }
}
