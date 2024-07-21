using System.Collections.Concurrent;
using System.Reflection;
using SandboxTest.Hosting.Internal;

namespace SandboxTest.Hosting
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
        /// Represents calls to record with specific arguments.
        /// </summary>
        public ConcurrentBag<ServiceInterceptorCallToRecord> CallsToRecord { get; set; } = new ConcurrentBag<ServiceInterceptorCallToRecord>();

        /// <summary>
        /// A list of all the recorded calls.
        /// </summary>
        public ConcurrentBag<object?[]?> CallRecodings { get; set; } = new ConcurrentBag<object?[]?>();

        /// <summary>
        /// The method to intercept.
        /// </summary>
        public MethodInfo InterceptedMethod { get => _interceptedMethod; }

        /// <summary>
        /// Configured method call replacers.
        /// </summary>
        public ConcurrentQueue<ServiceInterceptorCallReplacer> CallReplacers { get; set; } = new ConcurrentQueue<ServiceInterceptorCallReplacer>();
    }
}
