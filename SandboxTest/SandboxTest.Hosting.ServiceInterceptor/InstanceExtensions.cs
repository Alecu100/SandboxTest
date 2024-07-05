using SandboxTest.Instance;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="ServiceInterceptorController"/>.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Adds an application controller of type <see cref="ServiceInterceptorController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddServiceIncerpeptorController(this IInstance applicationInstance)
        {
            var hostBuilderApplicationRunner = applicationInstance.Runner as IHostRunner;
            if (hostBuilderApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected HostBuilderApplicationRunner");
            }
            if (applicationInstance.Controllers.Any(controller => controller is ServiceInterceptorController))
            {
                throw new InvalidOperationException("Application instance already has one service interceptor controller assigned, only one can be assigned per application instance");
            }

            var hostApplicationController = new ServiceInterceptorController();
            applicationInstance.AddController(hostApplicationController);
            return applicationInstance;
        }
    }
}
