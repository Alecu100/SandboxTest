namespace SandboxTest.Net.Http
{
    /// <summary>
    /// Represents an application controller that exposes a <see cref="HttpClient"/> to control an application instance via http requests.
    /// </summary>
    public class HttpClientApplicationController : IApplicationController
    {
        protected readonly string? _name;
        protected readonly Uri _baseAddress;
        protected HttpClient? _httpClient;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientApplicationController"/>
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="name"></param>
        public HttpClientApplicationController(string baseAddress, string? name) 
        {
            _name = name;
            _baseAddress = new Uri(baseAddress, UriKind.Absolute);
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
        public virtual Task BuildAsync(IApplicationInstance applicationInstance)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = _baseAddress;
            return Task.CompletedTask;
        }

        public virtual Task ConfigureBuildAsync(IApplicationInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        public virtual Task ResetAsync(ApplicationInstance applicationInstance)
        {
            return Task.CompletedTask;
        }
    }
}
