using SandboxTest.Instance;

namespace SandboxTest.WebServer
{
    /// <summary>
    /// Represents a runner that hosts a web server exposing the base address of the web server
    /// </summary>
    public interface IWebServerRunner : IRunner
    {
        /// <summary>
        /// The address that the web servers servers requests from.
        /// </summary>
        string Url { get; }
    }
}
