namespace GoldenCrown.Domain.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
    }
}
