using Microsoft.Extensions.Options;

namespace RateWatcher
{
    public class Worker(IOptions<RateWatcherOptions> options, IServiceScopeFactory seriveScope, ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var optionsCopy = options.Value;
            while (!stoppingToken.IsCancellationRequested)
            {
                var scope = seriveScope.CreateScope();
                var nbp = scope.ServiceProvider.GetRequiredService<NBPClient>();
                foreach (var currency in options.Value.Currency)
                {
                    var value = await nbp.GetCurrentRate(currency, stoppingToken);
                    try
                    {
                        logger.LogInformation(message: $"{currency}:{value?.rates[0].mid}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Error getting something fun to say: {Error}", ex);
                    }

                }
                    await Task.Delay(optionsCopy.RefreshInterval, stoppingToken);
            }

        }
    }
}
