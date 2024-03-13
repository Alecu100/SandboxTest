using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandboxTest.Sample.Application1
{
    public class ConsoleBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private PeriodicTimer _timer;

        public ConsoleBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = RunTimer();
            return Task.CompletedTask;
        }

        private async Task RunTimer()
        {
            while (await _timer.WaitForNextTickAsync()) 
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consoleService = _serviceProvider.GetRequiredService<IConsoleService>();
                    consoleService.WriteToConsole($"Running console background service");
                }
            }
        }
    }
}
