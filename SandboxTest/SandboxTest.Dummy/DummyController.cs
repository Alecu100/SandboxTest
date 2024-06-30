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
    }
}
