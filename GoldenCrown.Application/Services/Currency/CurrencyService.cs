using GoldenCrown.Infrastructure.Clients.ExchangeClient;

namespace GoldenCrown.Application.Services.Currency;

public interface ICurrencyService
{
    ValueTask<decimal> Convert(decimal amount, string currencyCode, string targetCurrencyCode, CancellationToken ct);
}

public class CurrencyService : ICurrencyService
{
    private readonly IExchangeClient _exchangeClient;

    public CurrencyService(IExchangeClient exchangeClient)
    {
        _exchangeClient = exchangeClient;
    }
    
    public async ValueTask<decimal> Convert(decimal amount, string currencyCode, string targetCurrencyCode, CancellationToken ct)
    {
        if (currencyCode == targetCurrencyCode)
        {
            return amount;
        }
        
        var rate = await _exchangeClient.GetExchangeRate(currencyCode, targetCurrencyCode, ct);
        return amount * rate;
    }
}