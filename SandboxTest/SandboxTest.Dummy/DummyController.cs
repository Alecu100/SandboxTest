using SandboxTest.Instance;

namespace SandboxTest.Dummy
{
    /// <summary>
    /// Dummy application controller class used more for test closer to unit tests rather than sandbox or integration tests.
    /// </summary>
    public class DummyController : ControllerBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="DummyController"/>
        /// </summary>
        /// <param name="name"></param>
        public DummyController(string? name) : base(name)
        {
        }
    }
}
