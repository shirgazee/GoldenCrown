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
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

            account!.Balance += request.Amount;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
