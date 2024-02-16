using Microsoft.Extensions.Hosting;

namespace SandboxTest.Hosting
{
    public interface IHostApplicationRunner
    {
        IHost Host { get; }

        IHostBuilder HostBuilder { get; }
    }
}
