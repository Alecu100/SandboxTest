namespace SandboxTest.Dummies
{
    /// <summary>
    /// Represents a dummy runner that does not do actually do anything used for tests that are more closer to unit tests than sandbox or integration tests.
    /// </summary>
    public class DummyApplicationRunner : IApplicationRunner
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
