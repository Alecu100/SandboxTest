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
                if (_controller.MethodInterceptors.ContainsKey(interfaceType) && _controller.MethodInterceptors[interfaceType].ContainsKey(targetMethod))
                {
                    var methodInterceptor = _controller.MethodInterceptors[interfaceType][targetMethod];

                    if (methodInterceptor.RecordsCall)
                    {
                        methodInterceptor.RecordedCalls.Add(args);
                    }

                    do
                    {
                        if (methodInterceptor.CallReplacers.TryPeek(out var callReplacer))
                        {
                            if (callReplacer.Times == int.MinValue)
                            {
                                if (callReplacer.CallReplaceFunc != null)
                                {
                                    return callReplacer.CallReplaceFunc(_wrappedInstance, args);
                                }
                                else if (callReplacer.CallReplaceAction != null)
                                {
                                    callReplacer.CallReplaceAction(_wrappedInstance, args);
                                    return null;
                                }
                                else
                                {
                                    throw new InvalidOperationException("No action or func set for call replacer");
                                }
                            }

                            int times = callReplacer.Times;
                            int remainingTimes = times--;
                            if (remainingTimes <= 0)
                            {
                                methodInterceptor.CallReplacers.TryDequeue(out _);
                                continue;
                            }
                            if (!(Interlocked.CompareExchange(ref callReplacer.Times, remainingTimes, times) == times))
                            {
                                continue;
                            }
                            if (callReplacer.Times == int.MinValue)
                            {
                                if (callReplacer.CallReplaceFunc != null)
                                {
                                    return callReplacer.CallReplaceFunc(_wrappedInstance, args);
                                }
                                else if (callReplacer.CallReplaceAction != null)
                                {
                                    callReplacer.CallReplaceAction(_wrappedInstance, args);
                                    return null;
                                }
                                else
                                {
                                    throw new InvalidOperationException("No action or func set for call replacer");
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (true);
                }
            }
            var result = targetMethod.Invoke(_wrappedInstance, args);

            return result;
        }
    }
}
