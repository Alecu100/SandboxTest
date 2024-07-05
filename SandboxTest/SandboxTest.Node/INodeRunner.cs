using SandboxTest.Instance;
using SandboxTest.WebServer;

namespace SandboxTest.Node
{
    /// <summary>
    /// Interface for node runners that start a web server hosted by node.js.
    /// </summary>
    public interface INodeRunner : IBuildableRunner, IWebServerRunner
    {
        /// <summary>
        /// Gets the base address of the node.js web server without port.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the port on which the node.js web servers is configured to use.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Returns true if the server uses ssl, false otherwise.
        /// </summary>
        bool UseSssl { get; }
    }
}
