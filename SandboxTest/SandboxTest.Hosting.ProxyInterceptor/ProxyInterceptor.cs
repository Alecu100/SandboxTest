using System.Reflection;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    public class ProxyInterceptor : DispatchProxy
    {
        private object? _wrappedInstance;
        private ProxyInterceptorController? _controller;
        private Type? _interfaceType;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (_interfaceType == null || _controller == null || _wrappedInstance == null)
            {
                throw new InvalidOperationException("Proxy interceptor not initialized");
            }
            if (targetMethod == null)
            {
                return null;
            }
            if (_controller.ProxyWrapperActions[_interfaceType].ContainsKey(targetMethod))
            {
                var methodActions = _controller.ProxyWrapperActions[_interfaceType][targetMethod];
                for (int actionIndex = methodActions.Count - 1; actionIndex  >= 0; actionIndex--) 
                {
                    ProxyInterceptorAction? action = methodActions[actionIndex];
                    if (action.ArgumentsMatcher(args))
                    {
                        if (targetMethod.ReturnParameter.ParameterType != typeof(void))
                        {
                            if (action.CallReplaceFunc == null)
                            {
                                throw new InvalidOperationException($"Could not find replacing func for interface type {_interfaceType.Name} and method {targetMethod.Name}");
                            }
                            return action.CallReplaceFunc(_wrappedInstance, args);
                        }
                        if (action.CallReplaceAction == null)
                        {
                            throw new InvalidOperationException($"Could not find replacing action for interface type {_interfaceType.Name} and method {targetMethod.Name}");
                        }
                        action.CallReplaceAction(_wrappedInstance, args);
                    }
                }
            }
            return targetMethod.Invoke(_wrappedInstance, args);
        }

        public void Initialize(ProxyInterceptorController controller, object? wrappedInstance, Type interfaceType)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
            _interfaceType = interfaceType;
        }
    }
}
