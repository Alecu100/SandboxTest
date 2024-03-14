using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// An application controller for HostApplicationRunner that exposes the IHost to execute operations on.
    /// </summary>
    public class HostApplicationController : IApplicationController
    {
        private readonly string? _name;

        /// <summary>
        /// Creates a new instance of the <see cref="HostApplicationController"/>
        /// </summary>
        /// <param name="name">The name of the controller, ca be empty to mark it as the default controller of that type</param>
        public HostApplicationController(string? name)
        {
            _name = name;
        }

        private IHost? _host;

        /// <summary>
        /// Gets the host to execute operations on.
        /// </summary>
        public IHost Host { get => _host ?? throw new InvalidOperationException("Host not initialized"); }

        /// <summary>
        /// Gets the 
        /// </summary>
        public string Name { get => nameof(HostApplicationController); }

        public Task BuildAsync(IApplicationInstance applicationInstance)
        {
            var hostApplicationRunner = applicationInstance.Runner as IHostApplicationRunner;
            if (hostApplicationRunner == null) 
            {
                throw new InvalidOperationException("Application instance has no host application runner assigned");
            }
            _host = hostApplicationRunner.Host;
            return Task.CompletedTask;
        }

        public Task ConfigureBuildAsync(IApplicationInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        public Task ResetAsync(ApplicationInstance applicationInstance)
        {
            return Task.CompletedTask;
        }
    }
}
