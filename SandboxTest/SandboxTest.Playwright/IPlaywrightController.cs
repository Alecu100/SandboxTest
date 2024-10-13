using Microsoft.Playwright;

namespace SandboxTest.Playwright
{
    public interface IPlaywrightController
    {
        /// <summary>
        /// Represents the browser type <see cref="PlaywrightControllerBrowserType"/>.
        /// </summary>
        public PlaywrightControllerBrowserType BrowserType { get; }

        /// <summary>
        /// Gets the visibility of the browser.
        /// </summary>
        public bool Headless { get; }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public IPage Page { get; }

        /// <summary>
        /// Gets the browser instance.
        /// </summary>
        public IBrowser Browser { get; }
    }
}
