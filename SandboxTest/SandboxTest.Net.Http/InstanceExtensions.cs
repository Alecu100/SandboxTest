using SandboxTest.WebServer;

namespace SandboxTest.Net.Http
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="HttpClientController"/>  and related functionalities.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Adds an application controller of type <see cref="HttpClientController"/> to the given  instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddHttpClientController(this IInstance applicationInstance, string? name = default)
        {
            if (applicationInstance.Runner == null)
            {
                throw new InvalidOperationException("Instance has no runner assigned");
            }
            var webServerRunner = applicationInstance.Runner as IWebServerRunner;
            if (webServerRunner == null)
            {
                throw new InvalidOperationException("Instance has no web server runner assigned");
            }

            var httpClientController = new HttpClientController(name);
            applicationInstance.AddController(httpClientController);
            return applicationInstance;
        }
    }
}
