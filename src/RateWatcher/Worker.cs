using Microsoft.Extensions.Options;

namespace RateWatcher
{
    public class Worker(IOptions<RateWatcherOptions> options, IServiceScopeFactory seriveScope, ILogger<Worker> logger, FileRateHistoryRepository fileRateController, RateChangeDetector rateChangeDetector) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var optionsCopy = options.Value;
            var lastRates = await fileRateController.GetNewestRate(cancellationToken);
            rateChangeDetector.InitLastRates(lastRates);


            while (!cancellationToken.IsCancellationRequested)
            {
                await using var scope = seriveScope.CreateAsyncScope();

                var nbp = scope.ServiceProvider.GetRequiredService<NBPClient>();
                foreach (var currency in options.Value.Currency)
                {
                    try
                    {
                        var data = await nbp.GetCurrentRate(currency, cancellationToken);
                        if (data != null)
                        {
                            if (rateChangeDetector.IsRateChange(data))
                            {
                                this.PintRate(data.code, data.rates[0].mid);

                                await fileRateController.SaveNewRateAsync(data.code, data.rates[0].mid, data.rates[0].effectiveDate, cancellationToken);

                            }
                        }
                        await Task.Delay(optionsCopy.RefreshInterval, cancellationToken);
                    }
                    catch (Exception e) when (!cancellationToken.IsCancellationRequested)
                    {
                        logger.LogError($"Error in {currency} {e.Message}");
                    }
                }
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
