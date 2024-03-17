using System.Linq.Expressions;
using System.Reflection;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    public class ProxyInterceptorConfigurator<TInterface>
    {
        private ProxyInterceptorController _controller;

        public ProxyInterceptorConfigurator(ProxyInterceptorController controller) 
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

        public ProxyWrapperConfiguratorAction<TInterface> ConfigureMethodCall<TReturn>(Expression<Func<TInterface, Action>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1>, Func<T1, bool>, Expression<Func<TInterface, Action<T1>>>> ConfigureMethodCall<T1>(Expression<Func<TInterface, Action<T1>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1>, Func<T1, bool>, Expression<Func<TInterface, Action<T1>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2>, Func<T1, T2, bool>, Expression<Func<TInterface, Action<T1, T2>>>> ConfigureMethodCall<T1, T2>(Expression<Func<TInterface, Action<T1, T2>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2>, Func<T1, T2, bool>, Expression<Func<TInterface, Action<T1, T2>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Action<T1, T2, T3>>>> ConfigureMethodCall<T1, T2, T3>(Expression<Func<TInterface, Action<T1, T2, T3>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Action<T1, T2, T3>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4>>>> ConfigureMethodCall<T1, T2, T3, T4>(Expression<Func<TInterface, Action<T1, T2, T3, T4>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>>> ConfigureMethodCall<T1, T2, T3, T4, T5>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5, T6>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>>> ConfigureMethodCall<T1, T2, T3, T4, T5, T6>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5, T6>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>>>(_controller, interfaceMethodFunc);
        }

        public ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>>> ConfigureMethodCall<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>> interfaceMethodFunc)
        {
            return new ProxyWrapperConfiguratorAction<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>>>(_controller, interfaceMethodFunc);
        }
    }

    public class ProxyWrapperConfiguratorFunc<TInterface, TReturn>
    {
        private ProxyInterceptorController _controller;
        private Expression<Func<TInterface, Func<TReturn>>> _interfaceMethodFunc;
        private Action<object>? _recordFunc;

        public ProxyWrapperConfiguratorFunc(ProxyInterceptorController controller, Expression<Func<TInterface, Func<TReturn>>> interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
        }

        public ProxyWrapperConfiguratorFunc<TInterface, TReturn> RecordsCall()
        {
            _recordFunc = (target) =>
            {
                _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod(), null));
            };
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
                CallReplaceFunc = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target);
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
                CallReplaceFunc = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target);
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
                CallReplaceFunc = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target);
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
        private ProxyInterceptorController _controller;
        private TArgumentFunc? _argumentsFunc;
        private Action<object, object?[]?>? _recordFunc;
        private TExpressionFunc _interfaceMethodFunc;

        public ProxyWrapperConfiguratorFunc(ProxyInterceptorController controller, TExpressionFunc interfaceMethodFunc)
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
                CallReplaceFunc = (target, args) =>
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
                CallReplaceFunc = (target, args) =>
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
                CallReplaceFunc = (target, args) =>
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

    public class ProxyWrapperConfiguratorAction<TInterface>
    {
        private ProxyInterceptorController _controller;
        private Expression<Func<TInterface, Action>> _interfaceMethodFunc;
        private Action<object>? _recordAction;

        public ProxyWrapperConfiguratorAction(ProxyInterceptorController controller, Expression<Func<TInterface, Action>> interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
        }

        public ProxyWrapperConfiguratorAction<TInterface> RecordsCall()
        {
            _recordAction = (target) =>
            {
                _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod(), null));
            };
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
                CallReplaceAction = (target, args) =>
                {
                    if (_recordAction != null)
                    {
                        _recordAction(target);
                    }
                    throw exception;
                }
            });
        }

        public void Calls(Action<object> callAction)
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
                CallReplaceAction = (target, args) =>
                {
                    if (_recordAction != null)
                    {
                        _recordAction(target);
                    }
                    callAction.Method.Invoke(target, null);
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

    public class ProxyWrapperConfiguratorAction<TInterface, TCallAction, TArgumentAction, TExpressionAction>
    where TCallAction : Delegate
    where TArgumentAction : Delegate
    where TExpressionAction : LambdaExpression

    {
        private ProxyInterceptorController _controller;
        private TArgumentAction? _argumentsFunc;
        private Action<object, object?[]?>? _recordFunc;
        private TExpressionAction _interfaceMethodAction;

        public ProxyWrapperConfiguratorAction(ProxyInterceptorController controller, TExpressionAction interfaceMethodAction)
        {
            _controller = controller;
            _interfaceMethodAction = interfaceMethodAction;
        }

        public ProxyWrapperConfiguratorAction<TInterface, TCallAction, TArgumentAction, TExpressionAction> WithArguments(TArgumentAction argumentsFunc)
        {
            _argumentsFunc = argumentsFunc;
            return this;
        }


        public ProxyWrapperConfiguratorAction<TInterface, TCallAction, TArgumentAction, TExpressionAction> RecordsCall()
        {
            _recordFunc = (target, args) =>
            {
                _controller.ProxyWrapperRecordedCalls.Add(new ProxyInterceptorRecordedCall(typeof(TInterface), GetInterfaceMethod(), args));
            };
            return this;
        }

        public void Calls(TCallAction callAction)
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
                CallReplaceAction = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, args);
                    }
                    callAction.Method.Invoke(target, args);
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
                CallReplaceAction = (target, args) =>
                {
                    if (_recordFunc != null)
                    {
                        _recordFunc(target, args);
                    }
                    throw exception;
                }
            });
        }

        private MethodInfo GetInterfaceMethod()
        {
            var interfaceExpressionBody = _interfaceMethodAction.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }
    }
}
