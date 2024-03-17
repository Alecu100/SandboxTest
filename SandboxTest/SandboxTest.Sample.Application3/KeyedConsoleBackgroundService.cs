using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SandboxTest.Sample.Application3
{
    public class KeyedConsoleBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private PeriodicTimer _timer;

        public KeyedConsoleBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
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
                    var guidGenerator = _serviceProvider.GetRequiredService<IRandomGuidGenerator>();
                    var consoleService = _serviceProvider.GetRequiredService<IConsoleService>();
                    consoleService.WriteToConsole($"Running console background service");
                    var keyedConsoleService = _serviceProvider.GetRequiredKeyedService<IConsoleService>("KeyedConsoleService");
                    keyedConsoleService.WriteToConsole(guidGenerator.GetNewGuid().ToString());
                }
            }
        }
    }
}
