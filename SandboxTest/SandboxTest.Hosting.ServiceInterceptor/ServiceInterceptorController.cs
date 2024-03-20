using Microsoft.Extensions.DependencyInjection;
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
        private IServiceProvider? _originalServiceProvider;

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
                var originalServices = new ServiceCollection();
                foreach (var service in services)
                {
                    originalServices.Add(service);
                }
                _originalServiceProvider = originalServices.BuildServiceProvider();
                services.Clear();
                services.AddSingleton(this);
                foreach (var serviceDescriptor in originalServices)
                {
                    if (serviceDescriptor.ServiceType.IsInterface && serviceDescriptor.ServiceType != typeof(IHostApplicationLifetime))
                    {
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationFactory != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType);
                            Func<IServiceProvider, object?, object> proxyInterceptorFactory = (provider, obj) =>
                            {
                                var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.KeyedImplementationFactory(provider, serviceDescriptor.ServiceKey) })
                                    ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationInstance != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType);
                            var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.KeyedImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, proxyInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationType != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType);
                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ServiceKey, serviceProxyInterceptorType, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                        }
                        if (serviceDescriptor.ImplementationFactory != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType);
                            Func<IServiceProvider, object> proxyInterceptorFactory = (provider) =>
                            {
                                var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.ImplementationFactory(provider) })
                                    ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");
                                return proxyInterceptor;
                            };

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptorFactory, serviceDescriptor.Lifetime);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationInstance != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType);
                            var proxyInterceptor = Activator.CreateInstance(serviceProxyInterceptorType, new object[] { this, serviceDescriptor.ImplementationInstance })
                                ?? throw new InvalidOperationException($"Failed to create service interceptor instance for type {serviceDescriptor.ServiceType.Name}");

                            var dispatchProxyServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, proxyInterceptor);
                            services.Add(dispatchProxyServiceDescriptor);
                            continue;
                        }
                        if (serviceDescriptor.ImplementationType != null)
                        {
                            var serviceProxyInterceptorType = ServiceInterceptor.CreateServiceInterceptorTypeWrapper(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);
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

        public ConcurrentDictionary<Type, ConcurrentDictionary<MethodInfo, List<ServiceInterceptorAction>>> ProxyWrapperActions { get => _proxyWrapperActions; }

        /// <summary>
        /// Returns a list of all the recored proxy wrapper calls.
        /// </summary>
        public ConcurrentBag<ServiceInterceptorRecordedCall> ProxyWrapperRecordedCalls { get => _proxyWrapperRecordedCalls; }
    }
}
