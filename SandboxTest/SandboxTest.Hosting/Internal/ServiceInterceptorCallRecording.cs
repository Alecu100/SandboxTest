﻿using System.Reflection;

namespace SandboxTest.Hosting.Internal
{
    /// <summary>
    /// Represents a recorded method call intercepted by a proxy interceptor.
    /// </summary>
    public class ServiceInterceptorCallRecording
    {
        private Type _interfaceType;
        private MethodInfo _method;
        private object?[]? _args;

        public ServiceInterceptorCallRecording(Type interfaceType, MethodInfo method, object?[]? args = null)
        {
            _interfaceType = interfaceType;
            _method = method;
            _args = args;
        }

        /// <summary>
        /// Returns the interface type that the proxy interceptor implements and the original object too.
        /// </summary>
        public Type InterfaceType { get => _interfaceType; }

        /// <summary>
        /// Returns the method that was invoked.
        /// </summary>
        public MethodInfo Method { get => _method; }

        /// <summary>
        /// Returns the arguments that the method was invoked with.
        /// </summary>
        public object?[]? Args { get => _args; }
    }
}
