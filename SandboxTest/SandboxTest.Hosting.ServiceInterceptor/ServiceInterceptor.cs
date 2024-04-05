using System.Reflection;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    public class ServiceInterceptor
    {
        public const string InvokeMethodName = nameof(Invoke);

        protected object? _wrappedInstance;
        protected ServiceInterceptorController _controller;
        protected List<Type> _implementedInterfaceTypes;

        public ServiceInterceptor(ServiceInterceptorController controller, Type wrappedType, object?[]? arguments)
        {
            _controller = controller;
            _implementedInterfaceTypes = GetType().GetInterfaces().ToList();
            if (wrappedType.IsGenericTypeDefinition)
            {
                if (!GetType().IsGenericType)
                {
                    throw new InvalidOperationException("Non generic service interceptor contains generic wrapped type");
                }
                var actualType = wrappedType.MakeGenericType(GetType().GetGenericArguments());
                _wrappedInstance = Activator.CreateInstance(actualType, arguments);
            }
            else
            {
                _wrappedInstance = Activator.CreateInstance(wrappedType, arguments);
            }
        }

        public ServiceInterceptor(ServiceInterceptorController controller, object wrappedInstance)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
            _implementedInterfaceTypes = GetType().GetInterfaces().ToList();
        }


        protected object? Invoke(MethodInfo? targetMethod, object?[]? args, Type[]? argsTypes)
        {
            if (_implementedInterfaceTypes == null || _controller == null || _wrappedInstance == null)
            {
                throw new InvalidOperationException("Proxy interceptor not initialized");
            }
            if (targetMethod == null)
            {
                throw new InvalidOperationException("Null target method");
            }
            var currentType = GetType();
            if (currentType.IsGenericType)
            {
                targetMethod = MethodBase.GetMethodFromHandle(targetMethod.MethodHandle, currentType.TypeHandle) as MethodInfo;
            }
            if (targetMethod == null)
            {
                throw new InvalidOperationException("Null target method");
            }
            foreach (var @interface in _implementedInterfaceTypes)
            {
                var interfaceMap = currentType.GetInterfaceMap(@interface);
                for (int i = 0; i < interfaceMap.TargetMethods.Length; i++) 
                {
                    var interfaceImplementedMethod = interfaceMap.TargetMethods[i];
                    if (interfaceImplementedMethod == targetMethod)
                    {
                        targetMethod = interfaceMap.InterfaceMethods[i];
                        break;
                    }
                }
            }
            if (targetMethod.IsGenericMethodDefinition)
            {
                if (argsTypes == null)
                {
                    throw new InvalidOperationException("No generic argument types passed for generic method");
                }
                targetMethod = targetMethod.MakeGenericMethod(argsTypes);
            }

            foreach (var interfaceType in _implementedInterfaceTypes)
            {
                if (_controller.ProxyWrapperActions.ContainsKey(interfaceType) && _controller.ProxyWrapperActions[interfaceType].ContainsKey(targetMethod))
                {
                    var methodActions = _controller.ProxyWrapperActions[interfaceType][targetMethod];
                    for (int actionIndex = methodActions.Count - 1; actionIndex >= 0; actionIndex--)
                    {
                        ServiceInterceptorAction? action = methodActions[actionIndex];
                        if (action.ArgumentsMatcher(args))
                        {
                            if (targetMethod.ReturnParameter.ParameterType != typeof(void))
                            {
                                if (action.CallReplaceFunc == null)
                                {
                                    return targetMethod.Invoke(_wrappedInstance, args);
                                }
                                return action.CallReplaceFunc(_wrappedInstance, args);
                            }
                            if (action.CallReplaceAction == null)
                            {
                                targetMethod.Invoke(_wrappedInstance, args);
                                return null;
                            }
                            action.CallReplaceAction(_wrappedInstance, args);
                        }
                    }
                }
            }

            return targetMethod.Invoke(_wrappedInstance, args);
        }
    }
}
