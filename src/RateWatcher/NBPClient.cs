using System.Net.Http.Json;
using System.Text.Json;

namespace RateWatcher
{
    public sealed class NBPClient(HttpClient httpClientFactory)
    {
        public async Task<NBPData?> GetCurrentRate(string currencyCode, CancellationToken cancellationToken)
        {

            var response = await httpClientFactory.GetFromJsonAsync<NBPData>($"A/{currencyCode}/?format=json",
                new JsonSerializerOptions(JsonSerializerDefaults.Web), cancellationToken);
            return response;
        }
    }
}
