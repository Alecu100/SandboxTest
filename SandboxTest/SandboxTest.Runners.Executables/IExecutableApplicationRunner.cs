using System.Diagnostics;

namespace SandboxTest.Runners.Executables
{
    public interface IExecutableApplicationRunner
    {
        /// <summary>
        /// Gets the process for the executable that is running.
        /// </summary>
        Process ExecutableProcess { get; }
    }
}
