using SandboxTest.Dummies;

namespace SandboxTest.WireMock
{
    public static class ApplicationInstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="DummyApplicationRunner"/> as the runner to the application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public static IApplicationInstance UseDummyApplicationRunner(this IApplicationInstance applicationInstance)
        {
            applicationInstance.UseRunner(new DummyApplicationRunner());
            return applicationInstance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="DummyApplicationController"/> to the given application instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationInstance AddDummyApplicationController(this IApplicationInstance applicationInstance, string? name = default)
        {
            var dummyApplicationRunner = applicationInstance.Runner as DummyApplicationRunner;
            if (dummyApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid application runner configured on application instance, expected DummyApplicationController");
            }

            var dummyApplicationController = new DummyApplicationController(name);
            applicationInstance.AddController(dummyApplicationController);
            return applicationInstance;
        }
    }
}
