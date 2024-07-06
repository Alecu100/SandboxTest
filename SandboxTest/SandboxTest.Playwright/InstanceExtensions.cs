using SandboxTest.Instance;
using SandboxTest.WebServer;
using System;

namespace SandboxTest.Playwright
{
    /// <summary>
    /// Static class that offers extension method to use the <see cref="PlaywrightController"/>.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Adds a controller of type <see cref="PlaywrightController"/> to the given instance that uses a <see cref="IWebServerRunner"/>.
        /// </summary>
        /// <param name="instance">The instance in which to add the controller.</param>
        /// <param name="browserType">The browser type, see <see cref="PlaywrightControllerBrowserType"/>.</param>
        /// <param name="name">Optionally specifies a name for the controller</param>
        /// <param name="startPageName">Optionally specifies the name of the initial page to navigate to.</param>
        /// <param name="slowMod">Specifies slow mode, to slow the page down.</param>
        /// <param name="headless">True if the browser is hidden, false to show the browser.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddPlaywrightController(this IInstance instance, PlaywrightControllerBrowserType browserType, string? name = default, string? startPageName = null, float? slowMod = null, bool headless = true)
        {
            if (instance.Runner == null)
            {
                throw new InvalidOperationException("Instance has no runner assigned");
            }
            var webServerRunner = instance.Runner as IWebServerRunner;
            if (webServerRunner == null)
            {
                throw new InvalidOperationException("Instance has no web server runner assigned");
            }

            var playwrightController = new PlaywrightController(name, browserType, startPageName, slowMod, headless);
            instance.AddController(playwrightController);
            return instance;
        }
    }
}
