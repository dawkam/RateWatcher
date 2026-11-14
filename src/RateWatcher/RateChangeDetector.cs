using Microsoft.Extensions.Options;

namespace RateWatcher
{

    public class RateChangeDetector(IOptions<RateWatcherOptions> options)
    {
        Dictionary<string, decimal> currentRate = new();

        public (bool, string, decimal) CheckRateChange(NBPData data)
        {
            decimal newValue = data.rates[0].mid;
            var optionsCopy = options.Value;
            var result = (false, "", 0m);

            if (!currentRate.TryGetValue(data.code, out decimal currentValue))
            {
                currentRate.Add(data.code, newValue);
                result = (true, data.code, newValue);
            }
            else if (IsChangeDetected(currentValue, newValue, optionsCopy.ValueChangeThreshold))
            {
                currentRate[data.code] = newValue;
                result = (true, data.code, newValue);
            }

            return result;
        }

        private bool IsChangeDetected(decimal currentRate, decimal newRate, decimal threshold)
        {
            return Math.Abs(currentRate - newRate) > threshold;
        }
    }
}
