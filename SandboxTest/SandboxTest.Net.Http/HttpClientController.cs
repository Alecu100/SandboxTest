using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.WebServer;

namespace SandboxTest.Net.Http
{
    /// <summary>
    /// Represents an application controller that exposes a <see cref="System.Net.Http.HttpClient"/> to control an instance via http requests.
    /// </summary>
    public class HttpClientController : IController
    {
        protected readonly string? _name;
        protected Uri? _baseAddress;
        protected HttpClient? _httpClient;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientController"/>
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="name"></param>
        public HttpClientController(string? name) 
        {
            _name = name;
        }

        ///<inheritdoc/>
        public string? Name { get => _name; }

        /// <summary>
        /// Returns an instance of the <see cref="System.Net.Http.HttpClient"/>
        /// </summary>
        public HttpClient HttpClient {  get => _httpClient ?? throw new InvalidOperationException("Controller not built"); }

        /// <summary>
        /// Builds the HttpClient application controller, creating the actual HttpClient instance.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), 10)]
        public virtual Task BuildAsync()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = _baseAddress;
            return Task.CompletedTask;
        }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWebServerRunner.RunAsync), -10)]
        public virtual Task ConfigureBuildAsync(IRunner runner)
        {
            var webServerRunner = runner as IWebServerRunner;
            if (webServerRunner == null)
            {
                throw new InvalidOperationException("Instance has no web server runner assigned");
            }
            _baseAddress = new Uri(webServerRunner.Url);
            return Task.CompletedTask;
        }
    }
}
