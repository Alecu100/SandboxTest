namespace SandboxTest.Hosting.ServiceInterceptor
{
    public class ServiceInterceptorMethodCallReplacer
    {
        required public Func<object?[]?, bool> ArgumentsMatcherFunc { get; set; }

        /// <summary>
        /// Replaces the call to a specific method for specific arguments with another call that returns something else or throws exception.
        /// </summary>
        public Func<object, object?[]?, object?>? CallReplaceFunc { get; set; }

        /// <summary>
        /// Replaces the call to a specific method for specific arguments with another call that can optionally throw exception.
        /// </summary>
        public Action<object, object?[]?>? CallReplaceAction { get; set; }

        /// <summary>
        /// The amounts of times to replace a call.
        /// </summary>
        public int Times;
    }
}
