using GoldenCrown.Database;
using GoldenCrown.Dtos.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.GetTransactionHistory
{
    public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, Result<List<TransactionHistoryResponse>>>
    {
        private readonly ApplicationDbContext _context;

        public GetTransactionHistoryQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TransactionHistoryResponse>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
        {
            if (request.DateFrom != null && request.DateTo != null && request.DateFrom > request.DateTo)
            {
                return Result<List<TransactionHistoryResponse>>.Failure("Некорректный диапазон дат");
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

            var transactions = _context.Transactions.Where(x => x.SenderAccountId == account!.Id || x.ReceiverAccountId == account.Id);

            if (request.DateFrom != null)
            {
                transactions = transactions.Where(x => x.CreatedAt >= request.DateFrom.Value);
            }
            if (request.DateTo != null)
            {
                transactions = transactions.Where(x => x.CreatedAt <= request.DateTo.Value);
            }
            transactions = transactions.Skip(request.Skip).Take(request.Take);

            var dbTransactions = await transactions.ToListAsync(cancellationToken);

            var allAccountIds = dbTransactions.Select(x => x.SenderAccountId)
                .Concat(dbTransactions.Select(x => x.ReceiverAccountId))
                .ToHashSet();

            var names = await _context.Accounts
                .Where(x => allAccountIds.Contains(x.Id))
                .Join(_context.Users,
                    acc => acc.UserId,
                    u => u.Id,
                    (acc, u) => new
                    {
                        Name = u.Name,
                        AccId = acc.Id,
                    })
                .ToDictionaryAsync(x => x.AccId, cancellationToken);

            var result = dbTransactions.Select(t => new TransactionHistoryResponse
            {
                SenderName = names[t.SenderAccountId].Name,
                ReceiverName = names[t.ReceiverAccountId].Name,
                Amount = t.Amount,
                Date = t.CreatedAt
            }).ToList();

            return Result<List<TransactionHistoryResponse>>.Success(result);
        }
    }
}
