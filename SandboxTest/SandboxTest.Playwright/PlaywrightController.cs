using Microsoft.Playwright;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Utils;
using SandboxTest.WebServer;

namespace SandboxTest.Playwright
{
    public class PlaywrightController : ControllerBase, IPlaywrightController
    {
        protected IPlaywright? _playwright;
        protected IPage? _page;
        protected IBrowser? _browser;
        protected bool _headless;
        protected PlaywrightControllerBrowserType _browserType;
        protected float? _sloMo;
        protected string? _startPageName;
        protected IWebServerRunner? _webServerRunner;

        /// <inheritdoc/>
        public PlaywrightControllerBrowserType BrowserType { get => _browserType; }

        /// <inheritdoc/>
        public bool Headless { get => _headless; }

        /// <inheritdoc/>
        public IPage Page { get => _page ?? throw new InvalidOperationException("Playwright controller not built"); }


        public PlaywrightController(string? name, PlaywrightControllerBrowserType browserType, string? startPageName = null, float? slowMod = null, bool headless = true) : base(name)
        {
            _browserType = browserType;
            _headless = headless;
            _sloMo = slowMod;
            _startPageName = startPageName;
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), -200)]
        public async Task ConfigureRunAsync()
        {
            await CommandLineUtils.RunCommandAsync($"powershell.exe \"{Environment.CurrentDirectory}\\playwright.ps1\" install");
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), 200)]
        public async Task RunAsync(IRunner runner)
        {
            _webServerRunner = runner as IWebServerRunner;
            if (_webServerRunner == null)
            {
                throw new InvalidOperationException("Expected a web server runner");
            }

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browserTypeLaunchOptions = new BrowserTypeLaunchOptions
            {
                Headless = _headless,
                SlowMo = _sloMo

            };
            if (_browserType == PlaywrightControllerBrowserType.Chromium)
            {
                _browser = await _playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);
            }
            else if (_browserType == PlaywrightControllerBrowserType.Firefox)
            {
                _browser = await _playwright.Firefox.LaunchAsync(browserTypeLaunchOptions);
            }
            else if (_browserType == PlaywrightControllerBrowserType.Webkit)
            {
                _browser = await _playwright.Webkit.LaunchAsync(browserTypeLaunchOptions);
            }
            _page = await _browser!.NewPageAsync();
            var startPageUri = $"{_webServerRunner.Url.TrimEnd('/').TrimEnd('\\')}{(_startPageName != null ? '\\' + _startPageName : string.Empty)}";
            await _page.GotoAsync(startPageUri);
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.StopAsync), -200)]
        public async Task StopAsync()
        {
            if (_browser == null || _playwright == null || _page == null)
            {
                throw new InvalidOperationException("Playwright controller not started");
            }
            await _browser.DisposeAsync();
            _playwright.Dispose();
        }
    }
}
