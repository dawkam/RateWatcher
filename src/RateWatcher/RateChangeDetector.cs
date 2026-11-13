using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace RateWatcher
{

    public class RateChangeDetector(IOptions<RateWatcherOptions> options) 
    {
        Dictionary<string, decimal> currentRate = new();

        public (bool, string , decimal ) CheckRateChange(NBPData data)
        {
            decimal newValue = data.rates[0].mid;
            var optionsCopy = options.Value;
            if (currentRate.TryGetValue(data.code, out decimal currentValue))
            {
                if (IsChangeDetected(currentValue, newValue, optionsCopy.ValueChangeThreshold))
                {
                    currentRate[data.code] = newValue;
                    return (true, data.code, newValue);
                }
            }
            else
            {
                currentRate.Add(data.code, newValue);
                return (true, data.code, newValue);
            }
            return (false,"",0);
        }

        private bool IsChangeDetected(decimal currentRate, decimal newRate, decimal threshold)
        {
            return Math.Abs(currentRate - newRate) > threshold;
        }
    }
}
