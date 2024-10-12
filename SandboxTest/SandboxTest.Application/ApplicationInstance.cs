using SandboxTest.Instance;

namespace SandboxTest.Application
{
    /// <summary>
    /// Represents a normal application instances hosted in the same process as the actual scenario suite and scenario.
    /// </summary>
    public class ApplicationInstance : InstanceBase
    {
        /// <summary>
        /// Creates an empty default application instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ApplicationInstance CreateEmptyInstance()
        {
            return new ApplicationInstance();
        }
    }
}
