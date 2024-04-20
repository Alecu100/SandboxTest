namespace SandboxTest.Net.Http
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="HttpClientController"/>  and related functionalities.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Adds an application controller of type <see cref="HttpClientController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddHttpClientController(this IInstance applicationInstance, string baseAddress, string? name = default)
        {
            if (applicationInstance.Runner == null)
            {
                throw new InvalidOperationException("Application has no runner assigned");
            }

            var wireMockApplicationController = new HttpClientController(baseAddress, name);
            applicationInstance.AddController(wireMockApplicationController);
            return applicationInstance;
        }
    }
}
