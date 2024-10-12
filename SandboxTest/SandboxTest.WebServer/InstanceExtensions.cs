using SandboxTest.Instance;

namespace SandboxTest.WebServer
{
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="RemoteWebServerRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="instance">The instance to which to add the remote web server runner.</param>
        /// <returns></returns>
        public static IInstance UseRemoteWebServerRunner(this IInstance instance)
        {
            instance.UseRunner(new RemoteWebServerRunner());
            return instance;
        }

        /// <summary>
        /// Configures the url for a <see cref="RemoteWebServerRunner"/>.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureRemoteWebServerRunnerUrl(this IInstance applicationInstance, string url)
        {
            var remoteWebServerRunner = applicationInstance.Runner as RemoteWebServerRunner;
            if (remoteWebServerRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected remote web server runner");
            }

            remoteWebServerRunner.OnConfigureUrl(url);
            return applicationInstance;
        }
    }
}
