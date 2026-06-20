using System.Text.Json;
using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public class DistributedCachedExchangeClient : IExchangeClient
{
    private readonly IExchangeClient _client;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCachedExchangeClient> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private static readonly DistributedCacheEntryOptions _options = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    public DistributedCachedExchangeClient(IExchangeClient client, IDistributedCache cache,
        ILogger<DistributedCachedExchangeClient> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<decimal> GetExchangeRate(string baseCurrencyCode, string targetCurrencyCode, CancellationToken ct)
    {
        var rates = await GetExchangeRates(baseCurrencyCode, ct);
        return rates.First(x => x.Quote == targetCurrencyCode).Rate;
    }

    public async Task<ExchageRateResponse[]> GetExchangeRates(string baseCurrencyCode, CancellationToken ct)
    {
        string key = $"currency:{baseCurrencyCode.ToUpper()}";
        var cached = await _cache.GetStringAsync(key, ct);
        if (cached != null)
        {
            _logger.LogInformation($"Currency cache hit for {baseCurrencyCode}");
            return JsonSerializer.Deserialize<ExchageRateResponse[]>(cached)!;
        }

        _logger.LogInformation($"Currency cache miss for {baseCurrencyCode}");

        ExchageRateResponse[] rates;
        await _semaphore.WaitAsync(ct);
        try
        {
            cached = await _cache.GetStringAsync(key, ct);
            if (cached != null)
            {
                _logger.LogInformation($"Currency cache hit after semaphore for {baseCurrencyCode}");
                return JsonSerializer.Deserialize<ExchageRateResponse[]>(cached)!;
            }

            _logger.LogInformation($"Currency http request for {baseCurrencyCode}");
            rates = await _client.GetExchangeRates(baseCurrencyCode, ct);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(rates), _options, ct);
        }
        finally
        {
            _semaphore.Release();
        }

        return rates;
    }
}