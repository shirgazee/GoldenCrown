namespace GoldenCrown.Infrastructure.Clients.ExchangeClient.Models;

public class ExchageRateResponse
{
    public DateTime Date { get; set; }
    public string Base { get; set; }
    public string Quote { get; set; }
    public decimal Rate { get; set; }
}