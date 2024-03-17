namespace SandboxTest.ProxyWrapper
{
    public class ProxyInterceptorAction
    {
        public int Order { get; set; }

        public Func<object[]?, bool>? ArgumentsMatcher { get; set; }

        /// <summary>
        /// Replaces the call to a specific method for specific arguments with another call that returns something else or throws exception.
        /// </summary>
        public Func<object, object[]?, object?>? CallReplace { get; set; }
    }
}
