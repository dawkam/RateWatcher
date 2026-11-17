using Microsoft.Extensions.Options;

namespace RateWatcher
{

    public class RateChangeDetector(IOptions<RateWatcherOptions> options)
    {
        private readonly Dictionary<string, decimal> currentRate = [];

        public void InitLastRates(Dictionary<string, decimal> lastRates)
        {
            this.currentRate.Clear();
            foreach (var rate in lastRates)
            {
                currentRate.Add(rate.Key, rate.Value);
            }
        }

        public bool IsRateChange(NBPData data)
        {
            decimal newValue = data.rates[0].mid;
            var optionsCopy = options.Value;
            var result = false;

            if (!currentRate.TryGetValue(data.code, out decimal currentValue))
            {
                currentRate.Add(data.code, newValue);
                result = true;

            }
            else if (IsRateReachThreshold(currentValue, newValue, optionsCopy.ValueChangeThreshold))
            {
                currentRate[data.code] = newValue;
                result = true;
            }

            return result;
        }

        private bool IsRateReachThreshold(decimal currentRate, decimal newRate, decimal threshold)
        {
            return Math.Abs(currentRate - newRate) > threshold;
        }
    }
}
