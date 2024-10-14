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
        bool UseSssl { get;}

        /// <summary>
        /// Gets the source path containing the javascript/typescript code to run in node.
        /// </summary>
        string SourcePath { get; set; }

        /// <summary>
        /// Gets or sets the functions used to parse the output from the node process to determine when it has finished starting and is ready to process requests.
        /// </summary>
        Func<string, bool> ParseReadyFunc { get; set; }

        /// <summary>
        /// Gets or sets the functions used to parse the output from the node process to determine when it has ran into an error.
        /// </summary>
        Func<string, bool> ParseErrorFunc { get; set; }

        /// <summary>
        /// Gets or sets the command used to run the node web server of choice.
        /// </summary>
        string NpmRunCommand { get; set; }
    }
}
