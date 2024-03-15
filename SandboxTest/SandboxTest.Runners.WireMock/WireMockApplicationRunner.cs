using SandboxTest;
using SandboxTest.WireMock;
using WireMock.Server;

namespace SanboxTest.Runners.WireMock
{
    /// <summary>
    /// Represents the WireMockApplicationRunner used to start instances of mocked Rest or http APIs with WireMockServer.
    /// </summary>
    public class WireMockApplicationRunner : IApplicationRunner, IWireMockApplicationRunner
    {
        protected int _port = 80;
        protected bool _useSsl = true;
        protected bool _useAdminInterface = false;
        protected Func<WireMockApplicationRunner, Task>? _configureBuildAction;
        protected WireMockServer? _wireMockServer;

        /// <summary>
        /// Returns the WireMock server set up for the application runner.
        /// </summary>
        public WireMockServer WireMockServer { get => _wireMockServer ?? throw new InvalidOperationException("WireMock application controller not built"); }

        /// <summary>
        /// Gets or sets the port to use start the WireMock server on.
        /// </summary>
        public int Port { get => _port; set => _port = value; }

        /// <summary>
        /// Gets or sets whether to use ssl or not.
        /// </summary>
        public bool UseSsl { get => _useSsl; set => _useSsl = value; }

        /// <summary>
        /// Gets or sets whether to use the admin interface.
        /// </summary>
        public bool UseAdminInterface { get => _useAdminInterface; set => _useAdminInterface = value; }

        /// <summary>
        /// BuildAsync is used to start WireMockRunner because the WireMockServer starts directly without any kind of build step aside from port and a couple of other things.
        /// </summary>
        /// <returns></returns>
        public Task BuildAsync()
        {
            if (_useAdminInterface)
            {
                _wireMockServer = WireMockServer.StartWithAdminInterface(_port, _useSsl);
            }
            else
            {
                _wireMockServer = WireMockServer.Start(_port, _useSsl);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// ConfigureBuildAsync is  not used for WireMockRunner because the WireMockServer starts directly without any kind of build step aside from port and a couple of other things.
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureBuildAsync()
        {
            if (_configureBuildAction != null)
            {
                await _configureBuildAction(this);
            }
        }

        /// <summary>
        /// ConfigureRunAsync is not used for WireMockRunner because it starts directly on build.
        /// </summary>
        /// <returns></returns>
        public virtual Task ConfigureRunAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the configure run function to configure on what port to start the WireMock server and if it should use ssl or admin interface.
        /// </summary>
        /// <param name="configureBuildFunc"></param>
        public void OnConfigureBuild(Func<WireMockApplicationRunner, Task>? configureBuildFunc)
        {
            _configureBuildAction = configureBuildFunc;
        }

        /// <summary>
        /// Resets the WireMockServer removing all the setup mock http responses.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task ResetAsync()
        {
            if (_wireMockServer == null)
            {
                throw new InvalidOperationException("Wiremock server not started");
            }
            _wireMockServer.Reset();
            return Task.CompletedTask;
        }

        /// <summary>
        /// WireMock server only start directly from the build step.
        /// </summary>
        /// <returns></returns>
        public virtual Task RunAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the WireMock server
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task StopAsync()
        {
            if (_wireMockServer == null)
            {
                throw new InvalidOperationException("Wiremock server not started");
            }
            _wireMockServer.Stop();
            return Task.CompletedTask;
        }
    }
}
