using Microsoft.Extensions.Options;

namespace RateWatcher
{
    public class Worker(IOptions<RateWatcherOptions> options,,ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var optionsCopy = options.Value;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(optionsCopy.RefreshInterval, stoppingToken);
            }
        }
    }
}
