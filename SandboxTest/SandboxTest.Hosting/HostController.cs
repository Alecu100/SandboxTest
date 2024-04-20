using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// An application controller for <see cref="HostController"/> that exposes the IHost to execute operations on.
    /// </summary>
    public class HostController : IController
    {
        private readonly string? _name;

        /// <summary>
        /// Creates a new instance of the <see cref="HostController"/>
        /// </summary>
        /// <param name="name">The name of the controller, ca be empty to mark it as the default controller of that type</param>
        public HostController(string? name)
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
        public string? Name { get => _name; }

        public Task BuildAsync(IInstance applicationInstance)
        {
            var hostApplicationRunner = applicationInstance.Runner as IHostRunner;
            if (hostApplicationRunner == null) 
            {
                throw new InvalidOperationException("Instance has no host runner assigned");
            }
            _host = hostApplicationRunner.Host;
            return Task.CompletedTask;
        }

        public Task ConfigureBuildAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }

        public Task ResetAsync(IInstance applicationInstance)
        {
            return Task.CompletedTask;
        }
    }
}
