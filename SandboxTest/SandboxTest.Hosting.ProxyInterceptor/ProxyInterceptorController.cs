﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    /// <summary>
    /// Represents a proxy wrapper controller that uses the dependency injection mechanism from standard IHost to replace all the implementations with proxy wrappers containing the original instance.
    /// These proxies intercept calls to them by default passing them to the original instance but also allows to intercept and verify these calls, return alternative values or even throw exceptions.
    /// </summary>
    public class ProxyInterceptorController : IApplicationController
    {
        private Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>> _proxyWrapperActions;
        private ConcurrentBag<ProxyInterceptorRecordedCall> _proxyWrapperRecordedCalls;
        private IServiceProvider? _originalServiceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="ProxyInterceptorController"/>.
        /// By default the name of it is always empty/null since only one of this kind of controller should be used per application instance.
        /// </summary>
        public ProxyInterceptorController()
        {
            _proxyWrapperActions = new Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>>();
            _proxyWrapperRecordedCalls = new ConcurrentBag<ProxyInterceptorRecordedCall>();
        }

        /// <summary>
        /// Starts to configure a proxy interceptor to return a specific value for a method call, throw an exception or just record the call.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public ProxyInterceptorConfigurator<TInterface> ConfigureInterceptor<TInterface>()
        {
            return new ProxyInterceptorConfigurator<TInterface>(this);
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
                var proxyInterceptorType = typeof(ProxyInterceptor);
                var originalServices = new ServiceCollection();
                foreach (var service in services)
                {
                    originalServices.Add(service);
                }
                _originalServiceProvider = originalServices.BuildServiceProvider();
                services.Clear();
                foreach (var serviceDescriptor in originalServices)
                {
                    if (serviceDescriptor.ServiceType.IsInterface)
                    {
                        if (!serviceDescriptor.IsKeyedService)
                        {
                            Func<IServiceProvider, object> proxyInterceptorFactory = (provider) =>
                            {
                                var proxyInterceptor = (ProxyInterceptor)DispatchProxy.Create(serviceDescriptor.ServiceType, proxyInterceptorType);
                                proxyInterceptor.Initialize(this, _originalServiceProvider.GetRequiredService(serviceDescriptor.ServiceType), serviceDescriptor.ServiceType);
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService)
                        {
                            Func<IServiceProvider, object?, object> proxyInterceptorFactory = (provider, key) =>
                            {
                                var proxyInterceptor = (ProxyInterceptor)DispatchProxy.Create(serviceDescriptor.ServiceType, proxyInterceptorType);
                                proxyInterceptor.Initialize(this, _originalServiceProvider.GetRequiredKeyedService(serviceDescriptor.ServiceType, key), serviceDescriptor.ServiceType);
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
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

        public Dictionary<Type, Dictionary<MethodInfo, List<ProxyInterceptorAction>>> ProxyWrapperActions { get => _proxyWrapperActions; }

        /// <summary>
        /// Returns a list of all the recored proxy wrapper calls.
        /// </summary>
        public ConcurrentBag<ProxyInterceptorRecordedCall> ProxyWrapperRecordedCalls { get => _proxyWrapperRecordedCalls; }
    }
}
