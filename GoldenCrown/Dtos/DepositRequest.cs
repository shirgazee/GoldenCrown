namespace GoldenCrown.API.Dtos
{
    public class DepositRequest
    {
        public decimal Amount { get; set; }
        
        public string Currency { get; set; }
    }
}
