using SandboxTest.Instance;

namespace SandboxTest.Dummy
{
    /// <summary>
    /// Represents a dummy runner that doesn't really run anything but instead it suited for tests more like unit tests.
    /// </summary>
    public class DummyRunner : RunnerBase, IRunner
    {
        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public override Task ResetAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public override Task RunAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public override Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
