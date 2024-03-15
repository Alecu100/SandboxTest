namespace SandboxTest.Net.Http
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="HttpClientApplicationController"/>  and related functionalities.
    /// </summary>
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Adds an application controller of type <see cref="HttpClientApplicationController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddHttpClientApplicationController(this IApplicationInstance applicationInstance, string baseAddress, string? name = default)
        {
            if (applicationInstance.Runner == null)
            {
                throw new InvalidOperationException("Application has no runner assigned");
            }

            var wireMockApplicationController = new HttpClientApplicationController(baseAddress, name);
            applicationInstance.AddController(wireMockApplicationController);
            return applicationInstance;
        }
    }
}
