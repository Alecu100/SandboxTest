using Microsoft.AspNetCore.Builder;

namespace SandboxTest.AspNetCore
{
    /// <summary>
    /// Represents a runner that exposes a WebApplication and a WebApplicationBuilder properties to be used by various application controllers.
    /// </summary>
    public interface IWebApplicationRunner
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
