using WireMock.Server;

namespace SandboxTest.WireMock
{
    public class WireMockController : IController
    {
        protected readonly string? _name;
        protected WireMockServer? _wireMockServer;

        /// <summary>
        /// Creates a new instance of the <see cref="WireMockController"/>.
        /// </summary>
        /// <param name="name"></param>
        public WireMockController(string? name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the WireMockApplicationController.
        /// </summary>
        public string? Name { get => _name; }

        /// <summary>
        /// Gets the WireMock server that can be configured to return specific responses for http requests.
        /// </summary>
        public WireMockServer WireMockServer { get => _wireMockServer ?? throw new InvalidOperationException("WireMock application controller is not built"); }

        /// <summary>
        /// Fetches the WireMock server instance in order to expose it to be configured to return specific responses for http requests.
        /// </summary>
        /// <param name="runner"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IWireMockRunner.RunAsync), 100)]
        public virtual Task RunAsync(IRunner runner)
        {
            var wireMockRunner = runner as IWireMockRunner;
            if (wireMockRunner == null)
            {
                throw new InvalidOperationException("WireMock application controller can only be used with a WireMock application runnner");
            }
            if (wireMockRunner.WireMockServer == null)
            {
                throw new InvalidOperationException($"WireMockRunner is not built");
            }
            _wireMockServer = wireMockRunner.WireMockServer;
            return Task.CompletedTask;
        }
    }
}
