﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Reflection;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    /// <summary>
    /// Represents a service proxy wrapper controller that uses the dependency injection mechanism from standard IHost to replace all the implementations with proxy wrappers containing the original instance.
    /// These proxies intercept calls to them by default passing them to the original instance but also allows to intercept and verify these calls, return alternative values or even throw exceptions.
    /// </summary>
    public class ServiceInterceptorController : IApplicationController
    {
        private ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, List<ServiceInterceptorAction>>> _proxyWrapperActions;
        private ConcurrentBag<ServiceInterceptorRecordedCall> _proxyWrapperRecordedCalls;
        private IServiceCollection _originalServiceCollection;
        private List<object> _referencesToKeepAlive = new List<object>();

        /// <summary>
        /// Creates a new instance of the <see cref="ServiceInterceptorController"/>.
        /// By default the name of it is always empty/null since only one of this kind of controller should be used per application instance.
        /// </summary>
        public ServiceInterceptorController()
        {
            _proxyWrapperActions = new ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, List<ServiceInterceptorAction>>>();
            _proxyWrapperRecordedCalls = new ConcurrentBag<ServiceInterceptorRecordedCall>();
        }

        /// <summary>
        /// Starts to configure a proxy interceptor to return a specific value for a method call, throw an exception or just record the call.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public ServiceInterceptorConfigurator<TInterface> ConfigureInterceptor<TInterface>()
        {
            return new ServiceInterceptorConfigurator<TInterface>(this);
        }

        /// <summary>
        /// Always returns null for the name of the controller, since there can't be multiple proxy controllers assigned to the same application instance.
        /// </summary>
        public string? Name { get => null; }

        public Task BuildAsync(IApplicationInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the IHostBuilder from the application instance and replaces all the service descriptio entries with new entries that wrap the instances or functions that return instances around proxy interceptors.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task ConfigureBuildAsync(IApplicationInstance applicationInstance)
        {
            var hostApplicationRunner = applicationInstance.Runner as IHostApplicationRunner;
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
                    if (serviceDescriptor.ServiceType.IsInterface && serviceDescriptor.ServiceType != typeof(IHostApplicationLifetime) && TypeIsAccessible(serviceDescriptor.ServiceType))
                    {
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationFactory != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, this);
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
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, this);
                            var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.KeyedImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, proxyInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationType != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType, this);
                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, serviceProxyInterceptorType, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationFactory != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, this);
                            Func<IServiceProvider, object> proxyInterceptorFactory = (provider) =>
                            {
                                var localServiceDescriptor = serviceDescriptor;
                                if (serviceDescriptor.ServiceType == typeof(IHost))
                                {
                                    return localServiceDescriptor.ImplementationFactory(provider);
                                }
                                var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, localServiceDescriptor.ImplementationFactory(provider) })
                                    ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationInstance != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, this);
                            var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.ImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationType != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, this);
                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceProxyInterceptorType, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
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
            var typeIsAccessible = type.IsPublic;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                foreach (var genericArgumentType in type.GetGenericArguments())
                {
                    typeIsAccessible &= TypeIsAccessible(genericArgumentType);
                }
            }

            return typeIsAccessible;
        }

        /// <summary>
        /// Clears all the proxy configured actions and the recorded calls.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public Task ResetAsync(ApplicationInstance applicationInstance)
        {
            _proxyWrapperActions.Clear();
            _proxyWrapperRecordedCalls.Clear();
            return Task.CompletedTask;
        }

        public void AddReference(object reference)
        {
            _referencesToKeepAlive.Add(reference);
        }

        public ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, List<ServiceInterceptorAction>>> ProxyWrapperActions { get => _proxyWrapperActions; }

        /// <summary>
        /// Returns a list of all the recored proxy wrapper calls.
        /// </summary>
        public ConcurrentBag<ServiceInterceptorRecordedCall> ProxyWrapperRecordedCalls { get => _proxyWrapperRecordedCalls; }
    }
}
