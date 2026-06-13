using GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;

namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public interface IExchangeClient
{
    Task<decimal> GetExchangeRate(string baseCurrencyCode, string targetCurrencyCode, CancellationToken ct);
    Task<ExchageRateResponse[]> GetExchangeRates(string baseCurrencyCode, CancellationToken ct);
}