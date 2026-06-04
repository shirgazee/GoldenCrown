using MediatR;

namespace GoldenCrown.Application.Features.Finance.Transfer
{
    public class TransferCommand : IRequest<Result>
    {
        public int FromUserId { get; }
        public string ToLogin { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string ReceiverCurrency { get; }

        public TransferCommand(int fromUserId, string toLogin, decimal amount, string currency,
            string receiverCurrency)
        {
            FromUserId = fromUserId;
            ToLogin = toLogin;
            Amount = amount;
            Currency = currency;
            ReceiverCurrency = receiverCurrency;
        }
    }
}
