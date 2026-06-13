using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public class CachedExchangeClient : IExchangeClient
{
    private readonly ExchangeClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedExchangeClient> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public CachedExchangeClient(ExchangeClient client, IMemoryCache cache, ILogger<CachedExchangeClient> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    private static readonly TimeSpan Ttl = TimeSpan.FromHours(1); // Ttl - time to live

    public async Task<decimal> GetExchangeRate(string baseCurrencyCode, string targetCurrencyCode, CancellationToken ct)
    {
        var rates = await GetExchangeRates(baseCurrencyCode, ct);
        return rates.First(x => x.Quote == targetCurrencyCode).Rate;
    }

    public async Task<ExchageRateResponse[]> GetExchangeRates(string baseCurrencyCode, CancellationToken ct)
    {
        string key = $"currency:{baseCurrencyCode.ToUpper()}";
        if (_cache.TryGetValue<ExchageRateResponse[]>(key, out var cached))
        {
            _logger.LogInformation($"Currency cache hit for {baseCurrencyCode}");
            return cached;
        }

        _logger.LogInformation($"Currency cache miss for {baseCurrencyCode}");

        ExchageRateResponse[] rates;
        await _semaphore.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(key, out cached))
            {
                _logger.LogInformation($"Currency cache hit after semaphore for {baseCurrencyCode}");
                return cached;
            }
        
            _logger.LogInformation($"Currency http request for {baseCurrencyCode}");
            rates = await _client.GetExchangeRates(baseCurrencyCode, ct);
            _cache.Set(key, rates, Ttl); 
        }
        finally
        {
            _semaphore.Release();
        }
        
        return rates;
    }
}