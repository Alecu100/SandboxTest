using SandboxTest.Executable;
using SandboxTest.WebServer;

namespace SandboxTest.Node
{
    /// <summary>
    /// Interface for node runners that start a web server hosted by node.js.
    /// </summary>
    public interface INodeRunner : IWebServerRunner
    {
        /// <summary>
        /// Gets the base address of the node.js web server without port.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Gets the port on which the node.js web servers is configured to use.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Thw kind of node server that the runner runs.
        /// </summary>
        NodeServerTypes ServerType { get; }
    }
}
