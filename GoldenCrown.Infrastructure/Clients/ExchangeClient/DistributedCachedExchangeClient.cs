using System.Text.Json;
using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;
using GoldenCrown.Infrastructure.Locking;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public class DistributedCachedExchangeClient : IExchangeClient
{
    private static readonly DistributedCacheEntryOptions _options = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly IExchangeClient _client;
    private readonly IDistributedCache _cache;
    private readonly IDistributedLock _distributedLock;
    private readonly ILogger<DistributedCachedExchangeClient> _logger;

    public DistributedCachedExchangeClient(
        IExchangeClient client,
        IDistributedCache cache,
        IDistributedLock distributedLock,
        ILogger<DistributedCachedExchangeClient> logger)
    {
        _client = client;
        _cache = cache;
        _distributedLock = distributedLock;
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

        #region Проверяем курсы в кеше и возвращаем если есть

        var cached = await _cache.GetStringAsync(key, ct);
        if (cached != null)
        {
            _logger.LogInformation($"Currency cache hit for {baseCurrencyCode}");
            return JsonSerializer.Deserialize<ExchageRateResponse[]>(cached)!;
        }

        #endregion

        _logger.LogInformation($"Currency cache miss for {baseCurrencyCode}");

        ExchageRateResponse[] rates;

        string lockKey = $"lock:{key}"; //  lock:currency:USD
        var lockTtl = TimeSpan.FromSeconds(10);
        var lockDeadline = DateTime.UtcNow + lockTtl;

        while (true)
        {
            await using var handle = await _distributedLock.TryAcquireLockAsync(lockKey, lockTtl, ct);

            #region Проверяем кеш еще раз, вдруг кто-то уже положил туда курсы

            cached = await _cache.GetStringAsync(key, ct);
            if (cached != null)
            {
                _logger.LogInformation($"Currency cache hit in distributed lock for {baseCurrencyCode}");
                return JsonSerializer.Deserialize<ExchageRateResponse[]>(cached)!;
            }

            #endregion

            #region Взяли лок и делаем запрос к внешнему сервису, сохраняем в кеш и возвращаем

            if (handle != null)
            {
                _logger.LogInformation($"Currency http request for {baseCurrencyCode}");
                rates = await _client.GetExchangeRates(baseCurrencyCode, ct);
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(rates), _options, ct);
                return rates;
            }

            #endregion

            #region Кто-то другой держит лок (handle == null), проверяем кеш еще раз

            cached = await _cache.GetStringAsync(key, ct);
            if (cached != null)
            {
                _logger.LogInformation($"Currency cache hit in distributed lock for {baseCurrencyCode}");
                return JsonSerializer.Deserialize<ExchageRateResponse[]>(cached)!;
            }

            #endregion

            await Task.Delay(TimeSpan.FromMicroseconds(100), ct);

            #region Если время лока истекло, то делаем запрос к внешнему сервису без кеша, чтобы не ждать слишком долго

            if (DateTime.UtcNow > lockDeadline)
            {
                _logger.LogInformation($"Currency lock timeount for {baseCurrencyCode}");
                return await _client.GetExchangeRates(baseCurrencyCode, ct);
            }

            #endregion
        }
    }
}