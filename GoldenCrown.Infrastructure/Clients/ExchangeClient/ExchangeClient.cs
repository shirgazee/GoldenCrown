using System.Net.Http.Json;
using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using Microsoft.Extensions.Options;

namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public class ExchangeClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ExchangeClientSettings _settings;

    public ExchangeClient(IOptions<ExchangeClientSettings> options, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<decimal> GetExchangeRate(string baseCurrencyCode, string targetCurrencyCode, CancellationToken ct)
    {
        return (await GetExchangeRates(baseCurrencyCode, ct)).First(x => x.Quote == targetCurrencyCode).Rate;
    }
    
    public async Task<ExchageRateResponse[]> GetExchangeRates(string baseCurrencyCode, CancellationToken ct)
    {
        var url = string.Format(_settings.Url, baseCurrencyCode);
        var result = await _httpClient.GetAsync(url, ct);
        result.EnsureSuccessStatusCode();
        var rates = await result.Content.ReadFromJsonAsync<ExchageRateResponse[]>(ct);
        
        return rates!;
    }
}