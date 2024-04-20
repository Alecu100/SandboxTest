namespace SandboxTest.Dummy
{
    /// <summary>
    /// Represents a dummy runner that doesn't really run anything but instead it suited for tests more like unit tests.
    /// </summary>
    public class DummyRunner : IRunner
    {
        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task BuildAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task ConfigureBuildAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task ConfigureRunAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task ResetAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
