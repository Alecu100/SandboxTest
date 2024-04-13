using System.Diagnostics;

namespace SandboxTest.Runners.Executables
{
    /// <summary>
    /// Interface for runners that start another executable in a separate process.
    /// </summary>
    public interface IExecutableApplicationRunner
    {
        /// <summary>
        /// Gets the process for the executable that is running.
        /// </summary>
        Process ExecutableProcess { get; }

        /// <summary>
        /// Gets the full file path of the executable that is being ran.
        /// </summary>
        string ExecutablePath { get; }
    }
}
