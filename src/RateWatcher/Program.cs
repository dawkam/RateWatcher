using RateWatcher;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddHttpClient<NBPClient>(client => client.BaseAddress = new Uri("https://api.nbp.pl/api/exchangerates/rates/"))
    .AddStandardResilienceHandler();
builder.Services.AddSingleton<RateChangeDetector>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOptions<RateWatcherOptions>().Bind(builder.Configuration.GetSection(nameof(RateWatcherOptions)));

var host = builder.Build();
var options = host.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RateWatcherOptions>>()
    .Value;

foreach (var currency in options.Currency)
{
    Console.WriteLine($"RateWatcherOptions.Currency={currency}");
}
Console.WriteLine($"RateWatcherOptions.RefreshInterval={options.RefreshInterval}");
Console.WriteLine($"RateWatcherOptions.ValueChangeThreshold={options.ValueChangeThreshold}");

host.Run();
