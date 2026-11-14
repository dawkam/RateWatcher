using Microsoft.Extensions.Options;

namespace RateWatcher.Tests
{
    public class UnitRateChangeTest
    {
        [Fact]
        public void Apply_FirstRateChange()
        {
            var options = Options.Create(new RateWatcherOptions { ValueChangeThreshold = 0.01m });
            var rateChangeDetector = new RateChangeDetector(options);
            string code = "USD";
            decimal value = 3.9885m;
            var nbpData = new NBPData()
            {
                code = code,
                rates = [new NBPRate()
                {
                    mid = value
                }]
            };

            var result = rateChangeDetector.CheckRateChange(nbpData);

            Assert.Equal((true, code, value), result);
        }

        [Theory]
        [InlineData(3.5, "USD", 0.001, false, 0)]
        [InlineData(3.6, "USD", 0.001, true, 3.6)]
        [InlineData(3.501, "USD", 0.001, false, 0)]
        [InlineData(3.501, "USD", 0.01, false, 0)]
        [InlineData(4, "EUR", 0.001, false, 0)]
        [InlineData(4.1, "EUR", 0.001, true, 4.1)]

        public void Apply_RateChange_ReturnNewRate(decimal newRate, string code, decimal threshold, bool isChanged, decimal rateAfterChange)
        {
            var options = Options.Create(new RateWatcherOptions { ValueChangeThreshold = threshold });
            var rateChangeDetector = InitTestData(options);
            var newNBPData = new NBPData()
            {
                code = code,
                rates = [new NBPRate()
                {
                    mid = newRate
                }]
            };
            var result = rateChangeDetector.CheckRateChange(newNBPData);
            Assert.Equal((isChanged, code, rateAfterChange), result);
        }


        private RateChangeDetector InitTestData(IOptions<RateWatcherOptions> options)
        {
            decimal initValue = 3.5m;
            var rateChangeDetector = new RateChangeDetector(options);
            var nbpDataUSD = new NBPData()
            {
                code = "USD",
                rates = [new NBPRate()
                {
                    mid = initValue
                }]
            };
            rateChangeDetector.CheckRateChange(nbpDataUSD);

            var nbpDataEUR = new NBPData()
            {
                code = "EUR",
                rates = [new NBPRate()
                {
                    mid = initValue + 0.5m
                }]
            };
            rateChangeDetector.CheckRateChange(nbpDataEUR);

            var nbpDataCHF = new NBPData()
            {
                code = "CHF",
                rates = [new NBPRate()
                {
                    mid = initValue + 1m
                }]
            };
            rateChangeDetector.CheckRateChange(nbpDataCHF);

            return rateChangeDetector;
        }

    }
}
