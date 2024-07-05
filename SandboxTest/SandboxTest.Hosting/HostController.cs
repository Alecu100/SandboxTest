using Microsoft.Extensions.Hosting;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Hosting
{
    /// <summary>
    /// An application controller for <see cref="HostController"/> that exposes the IHost to execute operations on.
    /// </summary>
    public class HostController : ControllerBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="HostController"/>
        /// </summary>
        /// <param name="name">The name of the controller, ca be empty to mark it as the default controller of that type</param>
        public HostController(string? name) : base(name)
        {
        }

        private IHost? _host;

        /// <summary>
        /// Gets the host to execute operations on.
        /// </summary>
        public IHost Host { get => _host ?? throw new InvalidOperationException("Host not initialized"); }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IHostRunner.BuildAsync), 10)]
        public Task BuildAsync(IRunner runner)
        {
            var hostApplicationRunner = runner as IHostRunner;
            if (hostApplicationRunner == null) 
            {
                throw new InvalidOperationException("Instance doesn't have a host runner assigned");
            }
            _host = hostApplicationRunner.Host;
            return Task.CompletedTask;
        }
    }
}
