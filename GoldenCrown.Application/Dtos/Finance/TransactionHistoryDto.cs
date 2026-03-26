namespace GoldenCrown.Application.Dtos.Finance;

public class TransactionHistoryDto
{
    public string SenderName { get; set; }
    public string ReceiverName { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Currency { get; set; }
}