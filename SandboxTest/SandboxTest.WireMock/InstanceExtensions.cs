using SanboxTest.Runners.WireMock;

namespace SandboxTest.WireMock
{
    /// <summary>
    /// Static class that offers extension methods to use the <see cref="WireMockRunner"/>  and related functionalities.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="WireMockRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IInstance UseWireMockRunner(this IInstance instance)
        {
            instance.UseRunner(new WireMockRunner());
            return instance;
        }

        /// <summary>
        /// Assigns a <see cref="WireMockRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IInstance ConfigureWireMockRunner(this IInstance instance, int port, bool useSsl, bool useAdminInterface)
        {
            if (instance.Runner == null)
            {
                throw new InvalidOperationException("Instance has no runner configured for it");
            }
            var wireMockRunner = instance.Runner as WireMockRunner;
            if (wireMockRunner == null) 
            {
                throw new InvalidOperationException("Instance doesn't use the WireMock runner");
            }
            wireMockRunner.OnConfigureBuild(runner =>
            {
                runner.UseSsl = useSsl;
                runner.UseAdminInterface = useAdminInterface;
                runner.Port = port;
                return Task.CompletedTask;
            });
            return instance;
        }

        /// <summary>
        /// Adds a controller of type <see cref="WireMockController"/> to the given instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddWireMockController(this IInstance instance, string? name = default)
        {
            var wireMockApplicationRunner = instance.Runner as WireMockRunner;
            if (wireMockApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected WireMockApplicationRunner");
            }

            var wireMockApplicationController = new WireMockController(name);
            instance.AddController(wireMockApplicationController);
            return instance;
        }
    }
}
