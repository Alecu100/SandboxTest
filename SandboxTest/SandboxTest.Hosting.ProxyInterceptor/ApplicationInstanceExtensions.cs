namespace SandboxTest.Hosting.ProxyInterceptor
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="ProxyInterceptorController"/>.
    /// </summary>
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Adds an application controller of type <see cref="ProxyInterceptorController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddProxyIncerpeptorController(this IApplicationInstance applicationInstance)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as IHostApplicationRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }
            if (applicationInstance.Controllers.Any(controller => controller is ProxyInterceptorController))
            {
                throw new InvalidOperationException("Application instance already has one interceptor controller assigned, only one can be assigned per application instance");
            }

            var hostApplicationController = new ProxyInterceptorController();
            applicationInstance.AddController(hostApplicationController);
            return applicationInstance;
        }
    }
}
