using MediatR;

namespace GoldenCrown.Application.Features.Finance.Transfer
{
    public class TransferCommand : IRequest<Result>
    {
        public int FromUserId { get; set; }
        public string ToLogin { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public TransferCommand(int fromUserId, string toLogin, decimal amount, string currency)
        {
            FromUserId = fromUserId;
            ToLogin = toLogin;
            Amount = amount;
            Currency = currency;
        }
    }
}
