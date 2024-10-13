using Microsoft.Playwright;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;
using SandboxTest.Utils;
using SandboxTest.WebServer;
using System.Diagnostics;

namespace SandboxTest.Playwright
{
    public class PlaywrightController : ControllerBase, IPlaywrightController
    {
        private const string Powershell = "pwsh";
        private static readonly string PowershellExecutableName;
        private static readonly bool PowershellFound;

        protected IPlaywright? _playwright;
        protected IPage? _page;
        protected IBrowser? _browser;
        protected bool _headless;
        protected PlaywrightControllerBrowserType _browserType;
        protected float? _sloMo;
        protected string? _startPageName;
        protected IWebServerRunner? _webServerRunner;
        protected bool _ignoreHttpsErrors;

        /// <inheritdoc/>
        public PlaywrightControllerBrowserType BrowserType { get => _browserType; }

        /// <inheritdoc/>
        public bool Headless { get => _headless; }

        /// <inheritdoc/>
        public IPage Page { get => _page ?? throw new InvalidOperationException("Playwright controller not built"); }

        /// <inheritdoc/>
        public IBrowser Browser { get => _browser ?? throw new InvalidOperationException("Playwright controller not built"); }

        static PlaywrightController()
        {
            PowershellExecutableName = Environment.OSVersion.Platform == PlatformID.Win32NT ? $"{Powershell}.exe" : Powershell;

            try
            {
                using var powershellProcess = Process.Start(PowershellExecutableName);
                PowershellFound = true;
            }
            catch (Exception)
            {
                PowershellFound = false;
            }
        }

        public PlaywrightController(string? name, PlaywrightControllerBrowserType browserType, string? startPageName = null, float? slowMod = null, bool headless = true, bool ignoreHttpsErrors = false) : base(name)
        {
            _browserType = browserType;
            _headless = headless;
            _sloMo = slowMod;
            _startPageName = startPageName;
            _ignoreHttpsErrors = ignoreHttpsErrors;
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), -200)]
        public async Task ConfigureRunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (PowershellFound == false)
            {
                throw new InvalidOperationException("Powershell core required but not found");
            }
            await CommandLineUtils.RunCommandAsync($"{Powershell} \"{Environment.CurrentDirectory}\\playwright.ps1\" install");
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), 200)]
        public async Task RunAsync(IRunner runner, IScenarioSuiteContext scenarioSuiteContext)
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

            _page = await _browser!.NewPageAsync(new BrowserNewPageOptions
            {
                IgnoreHTTPSErrors = _ignoreHttpsErrors,
                BaseURL = _webServerRunner.Url.TrimEnd('/').TrimEnd('\\')
            });
            await _page.GotoAsync(_startPageName ?? "\\");
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.StopAsync), -200)]
        public async Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
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
