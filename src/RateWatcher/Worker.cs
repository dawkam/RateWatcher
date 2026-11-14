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
                var scope = seriveScope.CreateScope();
                var rateChangeDetector = scope.ServiceProvider.GetRequiredService<RateChangeDetector>();
                var nbp = scope.ServiceProvider.GetRequiredService<NBPClient>();
                foreach (var currency in options.Value.Currency)
                {
                    var data = await nbp.GetCurrentRate(currency, cancellationToken);
                    if (data != null)
                    {
                        (bool isChanged, string code, decimal value) = await rateChangeDetector.CheckRateChange(data, cancellationToken);
                        if (isChanged)
                        {
                            this.PintRate(code, value);
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
