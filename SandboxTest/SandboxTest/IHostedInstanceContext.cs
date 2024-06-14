using System.Diagnostics;

namespace SandboxTest
{
    /// <summary>
    /// Represents the context in which a hosted instances runs enabling it to start another process.
    /// </summary>
    public interface IHostedInstanceContext
    {
        /// <summary>
        /// Launches a new process
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        Task<Process> LaunchProcessAsync(string filePath, bool debug, string? workingDirectory = null, string? arguments = null, IDictionary<string, string?>? environmentVariables = null);
    }
}
