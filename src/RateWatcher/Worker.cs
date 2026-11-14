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
                var rateChangeDetector = scope.ServiceProvider.GetRequiredService<RateChangeDetector>();
                var nbp = scope.ServiceProvider.GetRequiredService<NBPClient>();
                foreach (var currency in options.Value.Currency)
                {
                    var data = await nbp.GetCurrentRate(currency, stoppingToken);
                    if (data != null)
                    {
                        (bool isChanged, string code, decimal value) = rateChangeDetector.CheckRateChange(data);
                        if (isChanged)
                        {
                            this.PintRate(code, value);
                        }
                    }
                }
                await Task.Delay(optionsCopy.RefreshInterval, stoppingToken);
            }
        }



        private void PintRate(string currencyCode, decimal currencyValue)
        {

            try
            {
                logger.LogInformation(message: $"{currencyCode}:{currencyValue}");
            }
            catch (Exception ex)
            {
                logger.LogError("Error getting something fun to say: {Error}", ex);
            }
        }
    }
}
