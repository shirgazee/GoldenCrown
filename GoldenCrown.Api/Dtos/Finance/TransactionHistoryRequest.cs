namespace GoldenCrown.Api.Dtos.Finance
{
    public class TransactionHistoryRequest
    {
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}
