namespace GoldenCrown.Infrastructure.Clients.ExchangeClient;

public interface IExchangeClient
{
    Task<decimal> GetExchangeRate(string baseCurrencyCode, string targetCurrencyCode, CancellationToken ct);
}