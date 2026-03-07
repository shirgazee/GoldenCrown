using MediatR;

namespace GoldenCrown.Features.Finance.Transfer
{
    public class TransferCommand : IRequest<Result>
    {
        public int FromUserId { get; set; }
        public string ToLogin { get; set; }
        public decimal Amount { get; set; }

        public TransferCommand(int fromUserId, string toLogin, decimal amount)
        {
            FromUserId = fromUserId;
            ToLogin = toLogin;
            Amount = amount;
        }
    }
}
