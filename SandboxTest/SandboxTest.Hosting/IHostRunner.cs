using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Represents a runner that exposes a IHost and a IHostBuilder properties to be used by various application controllers.
    /// </summary>
    public interface IHostRunner : IBuildableRunner
    {
        /// <summary>
        /// Returns the host.
        /// </summary>
        IHost Host { get; }

        /// <summary>
        /// Returns the host builder.
        /// </summary>
        IHostBuilder HostBuilder { get; }
    }
}
