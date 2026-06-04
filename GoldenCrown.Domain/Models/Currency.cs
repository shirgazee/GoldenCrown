namespace GoldenCrown.Domain.Models;

public static class Currency
{
    public const string USD = "USD";
    public const string EUR = "EUR";
    public const string GBP = "GBP";
    
    public static readonly string[] AllCurrencies = [USD, EUR, GBP];
}