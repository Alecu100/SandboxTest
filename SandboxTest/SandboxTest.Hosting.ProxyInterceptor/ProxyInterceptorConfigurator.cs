using System.Linq.Expressions;
using System.Reflection;

namespace SandboxTest.ProxyWrapper
{
    public class ProxyInterceptorConfigurator<TInterface>
    {
        private ProxyIncerpeptorController _controller;

        public ProxyInterceptorConfigurator(ProxyIncerpeptorController controller) 
        { 
            _controller = controller;
        }

        public ProxyWrapperConfiguratorFunc<TInterface, TReturn> ConfigureMethodCall<TReturn>(Expression<Func<TInterface, Func<TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, TReturn>, Func<T1, bool>, Expression<Func<TInterface, Func<T1, TReturn>>>, TReturn> ConfigureMethodCall<T1, TReturn>(Expression<Func<TInterface, Func<T1, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, TReturn>, Func<T1, bool>, Expression<Func<TInterface, Func<T1, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, TReturn>, Func<T1, T2, bool>, Expression<Func<TInterface, Func<T1, T2, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, TReturn>(Expression<Func<TInterface, Func<T1, T2, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, TReturn>, Func<T1, T2, bool>, Expression<Func<TInterface, Func<T1, T2, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, TReturn>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, TReturn>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, TReturn>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, TReturn>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, TReturn>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, T5, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, TReturn>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, T5, T6, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, T5, T6, T7, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9,  TReturn>>>, TReturn> ConfigureMethodCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorFunc<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }
    }

    public class ProxyWrapperConfiguratorFunc<TInterface, TReturn>
    {
        private ProxyIncerpeptorController _controller;
        private Func<object, TReturn>? _callFunc;
        private Expression<Func<TInterface, Func<TReturn>>> _interfaceMethodFunc;
        private Action<object, object?[]?>? _recordFunc;

        public ProxyWrapperConfiguratorFunc(ProxyIncerpeptorController controller, Expression<Func<TInterface, Func<TReturn>>> interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
        }

        public ProxyWrapperConfiguratorFunc<TInterface, TReturn> RecordsCall()
        {
            var methodInfo = GetInterfaceMethod();
            if (_callFunc != null)
            {
                _callFunc = (target) =>
                {
                    _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod()));
                    return _callFunc(target);
                };
            }
            else
            {
                _callFunc = (target) =>
                {
                    _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod()));
                    return (TReturn)(methodInfo.Invoke(target, null) ?? throw new InvalidOperationException("Failed to invoke proxy method"));
                };
            }
            return this;
        }

        public void Throws<TException>(TException exception) where TException : Exception
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, null);
                    }
                    throw exception;
                }
            });
        }

        public void ReturnsValue(TReturn value)
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, null);
                    }
                    return value;
                }
            });
        }

        public void Calls(Func<object, TReturn> callFunc)
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, null);
                    }
                    return callFunc.Method.Invoke(target, null);
                }
            });
        }

        private MethodInfo GetInterfaceMethod()
        {
            var interfaceExpressionBody = _interfaceMethodFunc.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }
    }

    public class ProxyWrapperConfiguratorFunc<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn>
        where TCallFunc : Delegate
        where TArgumentFunc : Delegate
        where TExpressionFunc : LambdaExpression

    {
        private ProxyIncerpeptorController _controller;
        private TArgumentFunc? _argumentsFunc;
        private Action<object, object?[]?>? _recordFunc;
        private TExpressionFunc _interfaceMethodFunc;

        public ProxyWrapperConfiguratorFunc(ProxyIncerpeptorController controller, TExpressionFunc interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
        }

        public ProxyWrapperConfiguratorFunc<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> WithArguments(TArgumentFunc argumentsFunc)
        {
            _argumentsFunc = argumentsFunc;
            return this;
        }


        public ProxyWrapperConfiguratorFunc<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> RecordsCall()
        {
            _recordFunc = (target, args) =>
            {
                _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod(), args));
            };
            return this;
        }

        public void Calls(TCallFunc callFunc)
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    if (_argumentsFunc != null)
                    {
                        return (bool)(_argumentsFunc.Method.Invoke(null, args) ?? throw new InvalidOperationException("Could not invoke proxy argument filter function"));

                    }
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, args);
                    }
                    return callFunc.Method.Invoke(target, args);
                }
            });
        }

        public void Throws<TException>(TException exception) where TException : Exception
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    if (_argumentsFunc != null)
                    {
                        return (bool)(_argumentsFunc.Method.Invoke(null, args) ?? throw new InvalidOperationException("Could not invoke proxy argument filter function"));

                    }
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, args);
                    }
                    throw exception;
                }
            });
        }

        public void ReturnsValue(TReturn value)
        {
            var methodInfo = GetInterfaceMethod();
            var interfaceType = typeof(TInterface);
            if (!_controller.ProxyWrapperActions.ContainsKey(interfaceType))
            {
                _controller.ProxyWrapperActions[interfaceType] = new Dictionary<MethodInfo, List<ProxyInterceptorAction>>();
            }

            if (!_controller.ProxyWrapperActions[interfaceType].ContainsKey(methodInfo))
            {
                _controller.ProxyWrapperActions[interfaceType][methodInfo] = new List<ProxyInterceptorAction>();
            }

            _controller.ProxyWrapperActions[interfaceType][methodInfo].Add(new ProxyInterceptorAction
            {
                ArgumentsMatcher = (args) =>
                {
                    if (_argumentsFunc != null)
                    {
                        return (bool)(_argumentsFunc.Method.Invoke(null, args) ?? throw new InvalidOperationException("Could not invoke proxy argument filter function"));

                    }
                    return true;
                },
                CallReplace = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, args);
                    }
                    return value;
                }
            });
        }

        private MethodInfo GetInterfaceMethod()
        {
            var interfaceExpressionBody = _interfaceMethodFunc.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }
    }
}
