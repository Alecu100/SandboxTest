﻿using SandboxTest.Instance;
using SandboxTest.Scenario;
using WireMock.Server;

namespace SandboxTest.WireMock
{
    /// <summary>
    /// Runner used to used to start instances of mock Rest or http APIs with WireMockServer.
    /// </summary>
    public class WireMockRunner : RunnerBase, IWireMockRunner
    {
        protected int _port = 80;
        protected bool _useSsl = true;
        protected bool _useAdminInterface = false;
        protected WireMockServer? _wireMockServer;
        protected string _url;

        public WireMockRunner(int port = 80, bool useSsl = true, bool useAdminInterface = false)
        {
            _port = port;
            _useSsl = useSsl;
            _useAdminInterface = useAdminInterface;
            _url = $"{(_useSsl ? "https" : "http")}://127.0.0.1:{_port}";
        }

        /// <summary>
        /// Returns the WireMock server set up for the application runner.
        /// </summary>
        public WireMockServer WireMockServer { get => _wireMockServer ?? throw new InvalidOperationException("WireMock application controller not built"); }

        /// <summary>
        /// Gets or sets the port to use start the WireMock server on.
        /// </summary>
        public int Port { get => _port; }

        /// <summary>
        /// Gets or sets whether to use ssl or not.
        /// </summary>
        public bool UseSsl { get => _useSsl; }

        /// <summary>
        /// Gets or sets whether to use the admin interface.
        /// </summary>
        public bool UseAdminInterface { get => _useAdminInterface; }

        ///<inheritdoc/>
        public string Url => _url;

        /// <summary>
        /// Resets the WireMockServer removing all the setup mock http responses.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_wireMockServer == null)
            {
                throw new InvalidOperationException("Wiremock server not started");
            }
            _wireMockServer.Reset();
            return Task.CompletedTask;
        }

        /// <summary>
        /// RunAsync is used to start WireMockRunner because the WireMockServer starts directly without any kind of build step aside from port and a couple of other things.
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        public override Task RunAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_useAdminInterface)
            {
                _wireMockServer = WireMockServer.StartWithAdminInterface(_port, _useSsl);
            }
            else
            {
                _wireMockServer = WireMockServer.Start(_port, _useSsl);
            }
            _isRunning = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the WireMock server
        /// </summary>
        /// <param name="scenarioSuiteContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task StopAsync(IScenarioSuiteContext scenarioSuiteContext)
        {
            if (_wireMockServer == null)
            {
                throw new InvalidOperationException("Wiremock server not started");
            }
            _wireMockServer.Stop();
            _wireMockServer.Dispose();
            _isRunning = false;
            return Task.CompletedTask;
        }
    }
}
