using MediatR;

namespace GoldenCrown.Features.Finance.GetBalance
{
    public class GetBalanceQuery : IRequest<Result<decimal>>
    {
        public int UserId { get; set; }

        public GetBalanceQuery(int userId)
        {
            UserId = userId;
        }
    }
}
