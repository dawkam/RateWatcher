using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace RateWatcher
{
    public class FileRateHistoryRepository(IOptions<RateWatcherOptions> options, ILogger<Worker> logger)
    {
        private const string Header = "Code,Value,AddedOn";
        private const string DateFormat = "yyyy-MM-dd";
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        private readonly string path = Path.Combine(AppContext.BaseDirectory, "RateHistory.csv");


        Dictionary<string, List<(decimal value, DateOnly date)>> fileData = [];



        private async Task GetSavedRate(CancellationToken cancellationToken)
        {
            fileData = [];
            if (!File.Exists(path))
            {
                return;
            }

            var lines = await File.ReadAllLinesAsync(path, cancellationToken);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var columns = line.Split(',');
                if (columns.Length != 3
                || !decimal.TryParse(columns[1], NumberStyles.Number, Invariant, out var value)
                || !DateOnly.TryParseExact(columns[2], DateFormat, Invariant, DateTimeStyles.None, out var addedOn))
                {
                    continue;
                }

                if (fileData.TryGetValue(columns[0], out var data))
                {
                    fileData[columns[0]].Add((value, addedOn));
                }
                else
                {
                    var newList = new List<(decimal value, DateOnly date)>() {
                        (value, addedOn)
                    };
                    fileData.Add(columns[0], newList);
                    logger.LogInformation(message: $"Saved Before {columns[0]} : {value}");
                }
            }
        }

        public async Task<Dictionary<string, decimal>> GetNewestRate(CancellationToken cancellationToken)
        {
            Dictionary<string, decimal> result = [];
            if (fileData.Count == 0)
            {
                await GetSavedRate(cancellationToken);
                if (fileData.Count == 0)
                    return [];
            }
            var optionsCopy = options.Value;

            foreach (var item in fileData)
            {
                result.Add(item.Key, item.Value.MaxBy(x => x.date).value);
            }
            return result;
        }

        public async Task SaveNewRateAsync(string code, decimal value, DateOnly date, CancellationToken cancellationToken)
        {
            var line = new StringBuilder();

            if (!File.Exists(path))
            {
                line.AppendLine(Header);
            }

            line.Append(code).Append(',')
                .Append(value.ToString(Invariant)).Append(',')
                .AppendLine(date.ToString(DateFormat, Invariant));
            await File.AppendAllTextAsync(path, line.ToString(), cancellationToken);
        }
    }
}

