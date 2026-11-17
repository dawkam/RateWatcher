using Microsoft.Extensions.Options;

namespace RateWatcher
{
    public class Worker(IOptions<RateWatcherOptions> options, IServiceScopeFactory seriveScope, ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var optionsCopy = options.Value;
            while (!cancellationToken.IsCancellationRequested)
            {
                var scope = seriveScope.CreateAsyncScope();
                var rateChangeDetector = scope.ServiceProvider.GetRequiredService<RateChangeDetector>();
                var fileRateController = scope.ServiceProvider.GetRequiredService<FileRateController>();
                var lastRates = await fileRateController.GetNewestRate(cancellationToken);
                rateChangeDetector.InitLastRates(lastRates);

                var nbp = scope.ServiceProvider.GetRequiredService<NBPClient>();
                foreach (var currency in options.Value.Currency)
                {
                    var data = await nbp.GetCurrentRate(currency, cancellationToken);
                    if (data != null)
                    {
                        if (rateChangeDetector.IsRateChange(data))
                        {
                            this.PintRate(data.code, data.rates[0].mid);
                        }
                    }
                }
                await Task.Delay(optionsCopy.RefreshInterval, cancellationToken);
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
