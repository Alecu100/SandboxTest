using SandboxTest.WebServer;
using WireMock.Server;

namespace SandboxTest.WireMock
{
    /// <summary>
    /// Represents an application runner that exposes a WireMock server instance
    /// </summary>
    public interface IWireMockRunner : IWebServerRunner
    {
        /// <summary>
        /// Returns the WireMock server instance.
        /// </summary>
        WireMockServer WireMockServer { get; }
    }
}
