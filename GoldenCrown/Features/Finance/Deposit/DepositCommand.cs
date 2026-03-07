using MediatR;

namespace GoldenCrown.Features.Finance.Deposit
{
    public class DepositCommand : IRequest<Result>
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }

        public DepositCommand(int userId, decimal amount)
        {
            UserId = userId;
            Amount = amount;
        }
    }
}
