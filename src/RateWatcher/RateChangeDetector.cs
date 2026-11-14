using Microsoft.Extensions.Options;

namespace RateWatcher
{

    public class RateChangeDetector(IOptions<RateWatcherOptions> options, IServiceScopeFactory seriveScope, FileRateController fileRateController, TimeProvider timeProvider)
    {
        Dictionary<string, decimal> currentRate = [];

        public async Task<(bool, string, decimal)> CheckRateChange(NBPData data, CancellationToken cancellationToken)
        {
            decimal newValue = data.rates[0].mid;
            var optionsCopy = options.Value;
            var result = (false, data.code, 0m);

            if (currentRate.Count == 0)
            {
                currentRate = await fileRateController.GetNewestRate(cancellationToken);
            }


            if (!currentRate.TryGetValue(data.code, out decimal currentValue))
            {
                currentRate.Add(data.code, newValue);
                result = (true, data.code, newValue);
                var date = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                await fileRateController.SaveNewRateAsync(data.code, newValue, date, cancellationToken);

            }
            else if (IsChangeDetected(currentValue, newValue, optionsCopy.ValueChangeThreshold))
            {
                currentRate[data.code] = newValue;
                result = (true, data.code, newValue);
                var date = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                await fileRateController.SaveNewRateAsync(data.code, newValue, date, cancellationToken);

            }

            return result;
        }

        private bool IsChangeDetected(decimal currentRate, decimal newRate, decimal threshold)
        {
            return Math.Abs(currentRate - newRate) > threshold;
        }
    }
}
