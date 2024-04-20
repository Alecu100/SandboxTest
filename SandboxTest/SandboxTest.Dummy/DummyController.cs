namespace SandboxTest.Dummy
{
    /// <summary>
    /// Dummy application controller class used more for test closer to unit tests rather than sandbox or integration tests.
    /// </summary>
    public class DummyController : IController
    {
        private readonly string? _name;

        /// <summary>
        /// Creates a new instance of <see cref="DummyController"/>
        /// </summary>
        /// <param name="name"></param>
        public DummyController(string? name) 
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the dummy controller
        /// </summary>
        public string? Name { get => _name; }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="applicationInstance">Not actually used</param>
        /// <returns></returns>
        public Task BuildAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="applicationInstance">Not actually used</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throws invalid operation exception if it's not used with dummy application runner.</exception>
        public Task ConfigureBuildAsync(IInstance applicationInstance)
        {
            if (applicationInstance.Runner is not DummyRunner)
            {
                throw new InvalidOperationException("Dummy application controller can only be used with dummy application runner");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Just returns Task.Completed without doing anything else.
        /// </summary>
        /// <param name="applicationInstance">Not actually used</param>
        /// <returns></returns>
        public Task ResetAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }
    }
}
