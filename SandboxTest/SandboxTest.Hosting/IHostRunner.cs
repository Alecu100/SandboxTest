using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// Represents a runner that exposes a IHost and a IHostBuilder properties to be used by various application controllers.
    /// </summary>
    public interface IHostRunner : IRunner
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
