using SanboxTest.Runners.WireMock;

namespace SandboxTest.WireMock
{
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="WireMockApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public static IApplicationInstance UseWireMockApplicationRunner(this IApplicationInstance applicationInstance)
        {
            applicationInstance.UseRunner(new WireMockApplicationRunner());
            return applicationInstance;
        }

        /// <summary>
        /// Assigns a <see cref="WireMockApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public static IApplicationInstance ConfigureWireMockApplicationRunner(this IApplicationInstance applicationInstance, int port, bool useSsl, bool useAdminInterface)
        {
            if (applicationInstance.Runner == null)
            {
                throw new InvalidOperationException("Application instance has no runner configured for it");
            }
            var wireMockRunner = applicationInstance.Runner as WireMockApplicationRunner;
            if (wireMockRunner == null) 
            {
                throw new InvalidOperationException("Application instance doesn't use the WireMock application runner");
            }
            wireMockRunner.OnConfigureRun(runner =>
            {
                runner.UseSsl = useSsl;
                runner.UseAdminInterface = useAdminInterface;
                runner.Port = port;
                return Task.CompletedTask;
            });
            return applicationInstance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="WireMockApplicationController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddWireMockApplicationController(this IApplicationInstance applicationInstance, string? name = default)
        {
            var wireMockApplicationRunner = applicationInstance.Runner as WireMockApplicationRunner;
            if (wireMockApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WireMockApplicationRunner");
            }

            var wireMockApplicationController = new WireMockApplicationController(name);
            applicationInstance.AddController(wireMockApplicationController);
            return applicationInstance;
        }
    }
}
