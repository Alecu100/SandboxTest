﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Reflection;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    /// <summary>
    /// Represents a service proxy wrapper controller that uses the dependency injection mechanism from standard IHost to replace all the implementations with proxy wrappers containing the original instance.
    /// These proxies intercept calls to them by default passing them to the original instance but also allows to intercept and verify these calls, return alternative values or even throw exceptions.
    /// </summary>
    public class ServiceInterceptorController : IController
    {
        private ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>> _methodInterceptors;
        private ConcurrentBag<ServiceInterceptorRecordedCall> _proxyWrapperRecordedCalls;
        private IServiceCollection? _originalServiceCollection;
        private List<object> _referencesToKeepAlive = new List<object>();

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceInterceptorController"/>.
        /// By default the name of it is always empty/null since only one of this kind of controller should be used per application instance.
        /// </summary>
        public ServiceInterceptorController()
        {
            _methodInterceptors = new ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>>();
            _proxyWrapperRecordedCalls = new ConcurrentBag<ServiceInterceptorRecordedCall>();
        }

        /// <summary>
        /// Starts to configure a proxy interceptor to return a specific value for a method call, throw an exception or just record the call.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public ServiceInterceptorConfigurator<TInterface> UseInterceptor<TInterface>()
        {
            return new ServiceInterceptorConfigurator<TInterface>(this);
        }

        /// <summary>
        /// Always returns null for the name of the controller, since there can't be multiple proxy controllers assigned to the same application instance.
        /// </summary>
        public string? Name { get => null; }

        public Task BuildAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the IHostBuilder from the application instance and replaces all the service descriptio entries with new entries that wrap the instances or functions that return instances around proxy interceptors.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task ConfigureBuildAsync(IInstance applicationInstance)
        {
            var hostApplicationRunner = applicationInstance.Runner as IHostRunner;
            if (hostApplicationRunner == null)
            {
                throw new InvalidOperationException($"Target runner for application instance {applicationInstance.Id} must be a host application runner");
            }
            hostApplicationRunner.HostBuilder.ConfigureServices((ctx, services) =>
            {
                var proxyInterceptorType = typeof(ServiceInterceptor);
                _originalServiceCollection = new ServiceCollection();
                foreach (var service in services)
                {
                    _originalServiceCollection.Add(service);
                }
                services.Clear();
                services.AddSingleton(this);
                foreach (var serviceDescriptor in _originalServiceCollection)
                {
                    if (serviceDescriptor.ServiceType.IsInterface && TypeIsAccessible(serviceDescriptor.ServiceType))
                    {
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationFactory != null)
                        {
                            var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, this);
                            var serviceProxyInterceptorType = serviceInterceptorTypeBuilder.Build();
                            Func<IServiceProvider, object?, object> proxyInterceptorFactory = (provider, obj) =>
                            {
                                var localServiceDescriptor = serviceDescriptor;
                                var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, localServiceDescriptor.KeyedImplementationFactory(provider, serviceDescriptor.ServiceKey) })
                                    ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationInstance != null)
                        {
                            var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, this);
                            var serviceInterceptorType = serviceInterceptorTypeBuilder.Build();
                            var serviceInterceptor = Activator.CreateInstance(serviceInterceptorType, new object[] { this, serviceDescriptor.KeyedImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, serviceInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationType != null)
                        {
                            if (TypeIsAccessible(serviceDescriptor.KeyedImplementationType))
                            {
                                var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType, this);
                                var serviceInterceptorType = serviceInterceptorTypeBuilder.Build();
                                var serviceInterceptorServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, serviceInterceptorType, serviceDescriptor.Lifetime);
                                services.Add(serviceInterceptorServiceDescriptor);
                                continue;
                            }
                            else
                            {
                                services.Add(serviceDescriptor);
                                continue;
                            }
                        }
                        if (serviceDescriptor.ImplementationFactory != null)
                        {
                            var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, this);
                            var serviceInterceptorType = serviceInterceptorTypeBuilder.Build();
                            Func<IServiceProvider, object> proxyInterceptorFactory = (provider) =>
                            {
                                var serviceInterceptor = Activator.CreateInstance(serviceInterceptorType, new object[] { this, serviceDescriptor.ImplementationFactory(provider) })
                                    ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");
                                return serviceInterceptor;
                            };

                            var serviceInterceptorServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(serviceInterceptorServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationInstance != null)
                        {
                            var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, this);
                            var serviceInterceptorType = serviceInterceptorTypeBuilder.Build();
                            var serviceInterceptor = Activator.CreateInstance(serviceInterceptorType, new object[] { this, serviceDescriptor.ImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationType != null)
                        {
                            if (TypeIsAccessible(serviceDescriptor.ImplementationType))
                            {
                                var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, this);
                                var serviceInterceptorType = serviceInterceptorTypeBuilder.Build();
                                var serviceInterceptorServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceInterceptorType, serviceDescriptor.Lifetime);
                                services.Add(serviceInterceptorServiceDescriptor);
                            }
                            else
                            {
                                services.Add(serviceDescriptor);
                            }
                        }
                    }
                    else
                    {
                        services.Add(serviceDescriptor);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static bool TypeIsAccessible(Type type)
        {
            if (type == typeof(IHostApplicationLifetime))
            {
                return false;
            }

            if (!type.IsVisible)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clears all the proxy configured actions and the recorded calls.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public Task ResetAsync(IInstance applicationInstance)
        {
            _methodInterceptors.Clear();
            _proxyWrapperRecordedCalls.Clear();
            return Task.CompletedTask;
        }

        public void AddReference(object reference)
        {
            _referencesToKeepAlive.Add(reference);
        }

        public ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, ServiceInterceptedMethod>> MethodInterceptors { get => _methodInterceptors; }

        /// <summary>
        /// Returns a list of all the recored proxy wrapper calls.
        /// </summary>
        public ConcurrentBag<ServiceInterceptorRecordedCall> ProxyWrapperRecordedCalls { get => _proxyWrapperRecordedCalls; }
    }
}
