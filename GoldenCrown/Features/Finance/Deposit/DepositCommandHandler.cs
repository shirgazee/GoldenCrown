using GoldenCrown.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.Deposit
{
    public class DepositCommandHandler : IRequestHandler<DepositCommand, Result>
    {
        private readonly ApplicationDbContext _context;

        public DepositCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DepositCommand request, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId 
                                                                           && a.Currency == request.Currency, cancellationToken);
            if (account == null)
            {
                return Result.Failure("Account not found");
            }

            account!.Balance += request.Amount;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
