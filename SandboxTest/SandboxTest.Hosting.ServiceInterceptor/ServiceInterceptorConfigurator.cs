using SandboxTest.Hosting.ServiceInterceptor.Internal;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    public class ServiceInterceptorConfigurator<TInterface>
    {
        private ServiceInterceptorController _controller;

        public ServiceInterceptorConfigurator(ServiceInterceptorController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Configures service interceptor to intercept a method without parameters that just returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TReturn> Intercept<TReturn>(Expression<Func<TInterface, Func<TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with one parameter that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, TReturn>, Func<T1, bool>, Expression<Func<TInterface, Func<T1, TReturn>>>, TReturn> Intercept<T1, TReturn>(Expression<Func<TInterface, Func<T1, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, TReturn>, Func<T1, bool>, Expression<Func<TInterface, Func<T1, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 2 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, TReturn>, Func<T1, T2, bool>, Expression<Func<TInterface, Func<T1, T2, TReturn>>>, TReturn> Intercept<T1, T2, TReturn>(Expression<Func<TInterface, Func<T1, T2, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, TReturn>, Func<T1, T2, bool>, Expression<Func<TInterface, Func<T1, T2, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 3 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, TReturn>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>>, TReturn> Intercept<T1, T2, T3, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, TReturn>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Func<T1, T2, T3, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 4 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, TReturn>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, TReturn>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 5 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, TReturn>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, T5, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, TReturn>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 6 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, TReturn>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, T5, T6, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, TReturn>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 7 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, T5, T6, T7, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 8 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method with 9 parameters that returns a value.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>>, TReturn> Intercept<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Func<object, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>>>, TReturn>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and any parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface> Intercept<TReturn>(Expression<Func<TInterface, Action<object>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and one parameter.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1>, Func<T1, bool>, Expression<Func<TInterface, Action<T1>>>> Intercept<T1>(Expression<Func<TInterface, Action<T1>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1>, Func<T1, bool>, Expression<Func<TInterface, Action<T1>>>>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 2 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2>, Func<T1, T2, bool>, Expression<Func<TInterface, Action<T1, T2>>>> Intercept<T1, T2>(Expression<Func<TInterface, Action<T1, T2>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2>, Func<T1, T2, bool>, Expression<Func<TInterface, Action<T1, T2>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 3 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Action<T1, T2, T3>>>> Intercept<T1, T2, T3>(Expression<Func<TInterface, Action<T1, T2, T3>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3>, Func<T1, T2, T3, bool>, Expression<Func<TInterface, Action<T1, T2, T3>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 4 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4>>>> Intercept<T1, T2, T3, T4>(Expression<Func<TInterface, Action<T1, T2, T3, T4>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4>, Func<T1, T2, T3, T4, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 5 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>>> Intercept<T1, T2, T3, T4, T5>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5>, Func<T1, T2, T3, T4, T5, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 6 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>>> Intercept<T1, T2, T3, T4, T5, T6>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6>, Func<T1, T2, T3, T4, T5, T6, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 7 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>>> Intercept<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7>, Func<T1, T2, T3, T4, T5, T6, T7, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7>>>>(_controller, interfaceMethodFunc);
        }


        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 8 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7, T8>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8>>>> Intercept<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7, T8>, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8>>>>(_controller, interfaceMethodFunc);
        }

        /// <summary>
        /// Configures service interceptor to intercept a method without any return values and 9 parameters.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="interfaceMethodFunc"></param>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7, T8, T9>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>>> Intercept<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>> interfaceMethodFunc)
        {
            return new MethodInterceptorConfigurator<TInterface, Action<object, T1, T2, T3, T4, T5, T6, T7, T8, T9>, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>, Expression<Func<TInterface, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>>>>(_controller, interfaceMethodFunc);
        }
    }

    public class MethodInterceptorConfigurator<TInterface, TReturn>
    {
        private ServiceInterceptorController _controller;
        private Expression<Func<TInterface, Func<TReturn>>> _interfaceMethodFunc;
        private ServiceInterceptedMethod _interceptedMethod;
        private Type _interfaceType;

        public MethodInterceptorConfigurator(ServiceInterceptorController controller, Expression<Func<TInterface, Func<TReturn>>> interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
            _interfaceType = typeof(TInterface);

            var interceptedMethod = GetInterfaceInterceptedMethod();

            if (!_controller.MethodInterceptors.ContainsKey(_interfaceType))
            {
                _controller.MethodInterceptors.TryAdd(_interfaceType, new ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>());
            }
            if (!_controller.MethodInterceptors[_interfaceType].ContainsKey(interceptedMethod))
            {
                var serviceInterceptorMethod = new ServiceInterceptedMethod(interceptedMethod);
                if (_controller.MethodInterceptors[_interfaceType].TryAdd(interceptedMethod, serviceInterceptorMethod))
                {
                    _interceptedMethod = serviceInterceptorMethod;
                }
                else
                {
                    _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
                }
            }
            else
            {
                _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
            }
        }

        /// <summary>
        /// Resets the configured method interception, including recorded calls.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TReturn> Reset()
        {
            _interceptedMethod.RecordsCall = false;
            _interceptedMethod.CallReplacers.Clear();
            _interceptedMethod.RecordedCalls.Clear();

            return this;
        }

        /// <summary>
        /// Configures the interceptor to record calls to the specified method.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TReturn> RecordsAllCalls()
        {
            _interceptedMethod.RecordsCall = true;
            return this;
        }

        /// <summary>
        /// Checks that the specified method was called according to the timesFunc predicate. 
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimes(Func<int, bool> timesFunc)
        {
            var count = _interceptedMethod.RecordedCalls.Count();
            return timesFunc(count);
        }

        /// <summary>
        /// Checks that the specified method was called once.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesOnce()
        {
            return RecordedCallTimes(times => times == 1);
        }

        /// <summary>
        /// Checks that the specified method was never called.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesZeo()
        {
            return RecordedCallTimes(times => times == 0);
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> Throws<TException>(TException exception) where TException : Exception
        {
            return Throws(exception, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> Throws<TException>(TException exception, int times) where TException : Exception
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = args => true,
                CallReplaceFunc = (target, args) => throw exception,
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> ReturnsValue(TReturn value)
        {
            return ReturnsValue(value, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> ReturnsValue(TReturn value, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = args => true,
                CallReplaceFunc = (target, args) => value,
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> Calls(Func<object, TReturn> callFunc)
        {
            return Calls(callFunc, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TReturn> Calls(Func<object, TReturn> callFunc, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = args => true,
                CallReplaceFunc = (target, args) => callFunc(target),
                Times = times
            });
            return this;
        }

        private MethodInfo GetInterfaceInterceptedMethod()
        {
            var interfaceExpressionBody = _interfaceMethodFunc.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }
    }

    public class MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn>
        where TCallFunc : Delegate
        where TArgumentFunc : Delegate
        where TExpressionFunc : LambdaExpression

    {
        private ServiceInterceptorController _controller;
        private TArgumentFunc? _argumentsFunc;
        private TExpressionFunc _interfaceMethodFunc;
        private Type _interfaceType;
        private ServiceInterceptedMethod _interceptedMethod;

        public MethodInterceptorConfigurator(ServiceInterceptorController controller, TExpressionFunc interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
            _interfaceType = typeof(TInterface);

            var interceptedMethod = GetInterfaceInterceptedMethod();

            if (!_controller.MethodInterceptors.ContainsKey(_interfaceType))
            {
                _controller.MethodInterceptors.TryAdd(_interfaceType, new ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>());
            }
            if (!_controller.MethodInterceptors[_interfaceType].ContainsKey(interceptedMethod))
            {
                var serviceInterceptorMethod = new ServiceInterceptedMethod(interceptedMethod);
                if (_controller.MethodInterceptors[_interfaceType].TryAdd(interceptedMethod, serviceInterceptorMethod))
                {
                    _interceptedMethod = serviceInterceptorMethod;
                }
                else
                {
                    _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
                }
            }
            else
            {
                _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
            }
        }

        /// <summary>
        /// Resets the configured method interception, including recorded calls.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> Reset()
        {
            _interceptedMethod.RecordsCall = false;
            _interceptedMethod.CallReplacers.Clear();
            _interceptedMethod.RecordedCalls.Clear();

            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> WithArguments(TArgumentFunc argumentsFunc)
        {
            _argumentsFunc = argumentsFunc;
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> WithAnyArguments()
        {
            _argumentsFunc = null;
            return this;
        }

        /// <summary>
        /// Configures the interceptor to record calls to the specified method.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> RecordsAllCalls()
        {
            _interceptedMethod.RecordsCall = true;
            return this;
        }

        /// <summary>
        /// Checks that the specified method was called according to the timesFunc predicate.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimes(Func<int, bool> timesFunc)
        {
            int count = 0;
            if (_argumentsFunc != null)
            {
                count = _interceptedMethod.RecordedCalls.Count(x => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, x)!);
                return timesFunc(count);
            }
            else
            {
                count = _interceptedMethod.RecordedCalls.Count()!;
            }

            return timesFunc(count);
        }

        /// <summary>
        /// Checks that the specified method was called once.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesOnce()
        {
            return RecordedCallTimes(times => times == 1);
        }

        /// <summary>
        /// Checks that the specified method was never called.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesZero()
        {
            return RecordedCallTimes(times => times == 0);
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> Calls(TCallFunc callFunc)
        {
            return Calls(callFunc, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> Calls(TCallFunc callFunc, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = _argumentsFunc != null ? (args) => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, args)! : (args) => true,
                CallReplaceFunc = (target, args) => callFunc.Method.Invoke(callFunc.Target, GetCallFuncArguments(target, args)),
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> Throws<TException>(TException exception) where TException : Exception
        {
            return Throws(exception, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> Throws<TException>(TException exception, int times) where TException : Exception
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = _argumentsFunc != null ? (args) => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, args)! : (args) => true,
                CallReplaceFunc = (target, args) => throw exception,
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> ReturnsValue(TReturn value)
        {
            return ReturnsValue(value, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TCallFunc, TArgumentFunc, TExpressionFunc, TReturn> ReturnsValue(TReturn value, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = _argumentsFunc != null ? (args) => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, args)! : (args) => true,
                CallReplaceFunc = (target, args) => value,
                Times = times
            });
            return this;
        }

        private MethodInfo GetInterfaceInterceptedMethod()
        {
            var interfaceExpressionBody = _interfaceMethodFunc.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }

        private static object?[] GetCallFuncArguments(object target, object?[]? args)
        {
            var filledArgs = new List<object?>();
            filledArgs.Add(target);

            foreach (var arg in (args ?? Enumerable.Empty<object?>()))
            {
                filledArgs.Add(arg);
            }

            return filledArgs.ToArray();
        }
    }

    public class MethodInterceptorConfigurator<TInterface>
    {
        private ServiceInterceptorController _controller;
        private Expression<Func<TInterface, Action<object>>> _interfaceMethodFunc;
        private Type _interfaceType;
        private ServiceInterceptedMethod _interceptedMethod;

        public MethodInterceptorConfigurator(ServiceInterceptorController controller, Expression<Func<TInterface, Action<object>>> interfaceMethodFunc)
        {
            _controller = controller;
            _interfaceMethodFunc = interfaceMethodFunc;
            _interfaceType = typeof(TInterface);

            var interceptedMethod = GetInterfaceInterceptedMethod();

            if (!_controller.MethodInterceptors.ContainsKey(_interfaceType))
            {
                _controller.MethodInterceptors.TryAdd(_interfaceType, new ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>());
            }
            if (!_controller.MethodInterceptors[_interfaceType].ContainsKey(interceptedMethod))
            {
                var serviceInterceptorMethod = new ServiceInterceptedMethod(interceptedMethod);
                if (_controller.MethodInterceptors[_interfaceType].TryAdd(interceptedMethod, serviceInterceptorMethod))
                {
                    _interceptedMethod = serviceInterceptorMethod;
                }
                else
                {
                    _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
                }
            }
            else
            {
                _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
            }
        }

        /// <summary>
        /// Resets the configured method interception, including recorded calls.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface> Reset()
        {
            _interceptedMethod.RecordsCall = false;
            _interceptedMethod.CallReplacers.Clear();
            _interceptedMethod.RecordedCalls.Clear();

            return this;
        }

        /// <summary>
        /// Configures the interceptor to record calls to the specified method.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface> RecordsAllCalls()
        {
            _interceptedMethod.RecordsCall = true;

            return this;
        }

        /// <summary>
        /// Checks that the specified method was called according to the timesFunc predicate.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimes(Func<int, bool> timesFunc)
        {
            var count = _interceptedMethod.RecordedCalls.Count()!;
            return timesFunc(count);
        }

        /// <summary>
        /// Checks that the specified method was called once.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesOnce()
        {
            return RecordedCallTimes(times => times == 1);
        }

        /// <summary>
        /// Checks that the specified method was never called.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesZero()
        {
            return RecordedCallTimes(times => times == 0);
        }

        public MethodInterceptorConfigurator<TInterface> Throws<TException>(TException exception) where TException : Exception
        {
            return Throws(exception, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface> Throws<TException>(TException exception, int times) where TException : Exception
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = args => true,
                CallReplaceFunc = (target, args) => throw exception,
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface> Calls(Action<object> callAction)
        {
            return Calls(callAction, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface> Calls(Action<object> callAction, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = args => true,
                CallReplaceAction = (target, args) => callAction.Method.Invoke(callAction.Target, new object[] { target }),
                Times = times
            });
            return this;
        }

        private MethodInfo GetInterfaceInterceptedMethod()
        {
            var interfaceExpressionBody = _interfaceMethodFunc.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }
    }

    public class MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction>
    where TCallAction : Delegate
    where TArgumentAction : Delegate
    where TExpressionAction : LambdaExpression

    {
        private ServiceInterceptorController _controller;
        private TArgumentAction? _argumentsFunc;
        private TExpressionAction _interfaceMethodAction;
        private Type _interfaceType;
        private ServiceInterceptedMethod _interceptedMethod;

        public MethodInterceptorConfigurator(ServiceInterceptorController controller, TExpressionAction interfaceMethodAction)
        {
            _controller = controller;
            _interfaceMethodAction = interfaceMethodAction;
            _interfaceType = typeof(TInterface);

            var interceptedMethod = GetInterfaceMethod();

            if (!_controller.MethodInterceptors.ContainsKey(_interfaceType))
            {
                _controller.MethodInterceptors.TryAdd(_interfaceType, new ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>());
            }
            if (!_controller.MethodInterceptors[_interfaceType].ContainsKey(interceptedMethod))
            {
                var serviceInterceptorMethod = new ServiceInterceptedMethod(interceptedMethod);
                if (_controller.MethodInterceptors[_interfaceType].TryAdd(interceptedMethod, serviceInterceptorMethod))
                {
                    _interceptedMethod = serviceInterceptorMethod;
                }
                else
                {
                    _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
                }
            }
            else
            {
                _interceptedMethod = _controller.MethodInterceptors[_interfaceType][interceptedMethod];
            }
        }

        /// <summary>
        /// Resets the configured method interception, including recorded calls.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> Reset()
        {
            _interceptedMethod.RecordsCall = false;
            _interceptedMethod.CallReplacers.Clear();
            _interceptedMethod.RecordedCalls.Clear();

            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> WithArguments(TArgumentAction argumentsFunc)
        {
            _argumentsFunc = argumentsFunc;
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> WithAnyArguments()
        {
            _argumentsFunc = null;
            return this;
        }

        /// <summary>
        /// Configures the interceptor to record calls to the specified method.
        /// </summary>
        /// <returns></returns>
        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> RecordsAllCalls()
        {
            _interceptedMethod.RecordsCall = true;
            return this;
        }

        /// <summary>
        /// Checks that the specified method was called according to the timesFunc predicate.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimes(Func<int, bool> timesFunc)
        {
            int count = 0;
            if (_argumentsFunc != null)
            {
                count = _interceptedMethod.RecordedCalls.Count(x => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, x)!);
                return timesFunc(count);
            }
            else
            {
                count = _interceptedMethod.RecordedCalls.Count()!;
            }

            return timesFunc(count);
        }

        /// <summary>
        /// Checks that the specified method was called once.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesOnce()
        {
            return RecordedCallTimes(times => times == 1);
        }

        /// <summary>
        /// Checks that the specified method was never called.
        /// Optionally, arguments can be specified beforehand using the <see cref="WithArguments"/> method.
        /// </summary>
        /// <param name="timesFunc"></param>
        /// <returns></returns>
        public bool RecordedCallTimesZero()
        {
            return RecordedCallTimes(times => times == 0);
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> Calls(TCallAction callAction)
        {
            return Calls(callAction, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> Calls(TCallAction callAction, int times)
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = _argumentsFunc != null ? (args) => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, args)! : (args) => true,
                CallReplaceAction = (target, args) => callAction.Method.Invoke(callAction.Target, GetCallFuncArguments(target, args)),
                Times = times
            });
            return this;
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> Throws<TException>(TException exception) where TException : Exception
        {
            return Throws(exception, int.MinValue);
        }

        public MethodInterceptorConfigurator<TInterface, TCallAction, TArgumentAction, TExpressionAction> Throws<TException>(TException exception, int times) where TException : Exception
        {
            _interceptedMethod.CallReplacers.Enqueue(new ServiceInterceptorMethodCallReplacer
            {
                ArgumentsMatcherFunc = _argumentsFunc != null ? (args) => (bool)_argumentsFunc.Method.Invoke(_argumentsFunc.Target, args)! : (args) => true,
                CallReplaceAction = (target, args) => throw exception,
                Times = times
            });
            return this;
        }

        private MethodInfo GetInterfaceMethod()
        {
            var interfaceExpressionBody = _interfaceMethodAction.Body as UnaryExpression;
            var interfaceOperandExpression = interfaceExpressionBody?.Operand;
            var methodCallExpression = interfaceOperandExpression as MethodCallExpression;
            var methodInfo = methodCallExpression?.Object as ConstantExpression;

            return methodInfo?.Value as MethodInfo ?? throw new InvalidOperationException("Failed to get interface method to execute proxy actions on");
        }

        private static object?[] GetCallFuncArguments(object target, object?[]? args)
        {
            var filledArgs = new List<object?>();
            filledArgs.Add(target);

            foreach (var arg in (args ?? Enumerable.Empty<object?>()))
            {
                filledArgs.Add(arg);
            }
            return filledArgs.ToArray();
        }
    }
}
