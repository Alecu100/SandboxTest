using System.Collections.Concurrent;
using System.Reflection;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    /// <summary>
    /// Represents the configured interceptor for a specific method of a type.
    /// </summary>
    public class ServiceInterceptedMethod
    {
        private readonly MethodInfo _interceptedMethod;

        /// <summary>
        /// Creates a new instance of <see cref="ServiceInterceptedMethod"/>
        /// </summary>
        /// <param name="interceptedMethod"></param>
        public ServiceInterceptedMethod(MethodInfo interceptedMethod) 
        {
            _interceptedMethod = interceptedMethod;
        }

        /// <summary>
        /// Tells the incerceptor to record method calls.
        /// </summary>
        public bool RecordsCall { get; set; }

        /// <summary>
        /// A list of all the recorded calls.
        /// </summary>
        public ConcurrentBag<object?[]?> RecordedCalls { get; set; } = new ConcurrentBag<object?[]?>();

        /// <summary>
        /// The method to intercept.
        /// </summary>
        public MethodInfo InterceptedMethod { get => _interceptedMethod; }

        /// <summary>
        /// Configured method call replacers.
        /// </summary>
        public ConcurrentQueue<ServiceInterceptorMethodCallReplacer> CallReplacers { get; set; } = new ConcurrentQueue<ServiceInterceptorMethodCallReplacer>();
    }
}
