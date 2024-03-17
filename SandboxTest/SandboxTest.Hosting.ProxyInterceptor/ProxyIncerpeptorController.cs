using System.Collections.Concurrent;
using System.Reflection;

namespace SandboxTest.ProxyWrapper
{
    /// <summary>
    /// Represents a proxy wrapper controller that uses the dependency injection mechanism from standard IHost to replace all the implementations with proxy wrappers containing the original instance.
    /// These proxies intercept calls to them by default passing them to the original instance but also allows to intercept and verify these calls, return alternative values or even throw exceptions.
    /// </summary>
    public class ProxyIncerpeptorController : IApplicationController
    {
        private Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>> _proxyWrapperActions;
        private ConcurrentBag<ProxyInterceptorRecordedCall> _proxyWrapperRecordedCalls;

        /// <summary>
        /// Creates a new instance of the <see cref="ProxyIncerpeptorController"/>.
        /// By default the name of it is always empty/null since only one of this kind of controller should be used per application instance.
        /// </summary>
        public ProxyIncerpeptorController()
        {
            _proxyWrapperActions = new Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>>();
            _proxyWrapperRecordedCalls = new ConcurrentBag<ProxyInterceptorRecordedCall>();
        }

        /// <summary>
        /// Starts to configure a proxy wrapper to return a specific value for a method call, throw an exception or just record the call.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public ProxyInterceptorConfigurator<TInterface> ConfigureWrapper<TInterface>()
        {
            return new ProxyInterceptorConfigurator<TInterface>(this);
        }

        /// <summary>
        /// Always returns null for the name of the controller, since there can't be multiple proxy controllers assigned to the same application instance.
        /// </summary>
        public string? Name { get => null; }

        public Task BuildAsync(IApplicationInstance applicationInstance)
        {
            throw new NotImplementedException();
        }

        public Task ConfigureBuildAsync(IApplicationInstance applicationInstance)
        {
            throw new NotImplementedException();
        }

        public Task ResetAsync(ApplicationInstance applicationInstance)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>> ProxyWrapperActions { get => _proxyWrapperActions; }

        /// <summary>
        /// Returns a list of all the recored proxy wrapper calls.
        /// </summary>
        public ConcurrentBag<ProxyInterceptorRecordedCall> ProxyWrapperRecordedCalls { get => _proxyWrapperRecordedCalls; }
    }
}
