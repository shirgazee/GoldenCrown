namespace GoldenCrown.Api.Dtos.Finance
{
    public class TransactionHistoryResponse
    {
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public decimal Sum { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; set; }
    }
}
