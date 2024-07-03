using Microsoft.AspNetCore.Builder;
using SandboxTest.Hosting;

namespace SandboxTest.AspNetCore
{
    /// <summary>
    /// An web controller for <see cref="WebApplicationRunner"/> that exposes the <see cref="WebApplication"/> to execute operations on.
    /// </summary>
    public class WebApplicationController : IController
    {
        private readonly string? _name;

        /// <summary>
        /// Creates a new instance of the <see cref="WebApplicationController"/>
        /// </summary>
        /// <param name="name">The name of the controller, ca be empty to mark it as the default controller of that type</param>
        public WebApplicationController(string? name)
        {
            _name = name;
        }

        private WebApplication? _webApplication;

        /// <summary>
        /// Gets the web application to execute operations on.
        /// </summary>
        public WebApplication WebApplication { get => _webApplication ?? throw new InvalidOperationException("Web application not initialized"); }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string? Name { get => _name; }

        [AttachedMethod(AttachedMethodType.ControllerToRunner, nameof(IHostRunner.BuildAsync), 20)]
        public Task BuildAsync(IRunner runner)
        {
            var webApplicationRunner = runner as IWebApplicationRunner;
            if (webApplicationRunner == null)
            {
                throw new InvalidOperationException("Instance doesn't have a web application runner assigned");
            }
            _webApplication = webApplicationRunner.WebApplication;
            return Task.CompletedTask;
        }
    }
}
