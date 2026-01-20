using GoldenCrown.Database;
using GoldenCrown.Dtos.Finance;
using GoldenCrown.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GoldenCrown.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly ApplicationDbContext _dbContext;

        public FinanceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<decimal>> GetBalanceAsync(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == user!.Id);
            return Result<decimal>.Success(account!.Balance);
        }

        public async Task<Result> DepositAsync(int userId, decimal amount)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == user!.Id);

            account!.Balance += amount;
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> TransferAsync(int fromUserId, string toLogin, decimal amount)
        {
            var fromUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == fromUserId);
            var fromAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == fromUser!.Id);
            var toUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == toLogin);
            if (toUser == null)
            {
                return Result.Failure("Получатель не найден");
            }
            var toAccount = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == toUser.Id);
            if (fromAccount!.Balance < amount)
            {
                return Result.Failure("Недостаточно средств");
            }
            fromAccount.Balance -= amount;
            toAccount!.Balance += amount;

            var transaction = new Transaction
            {
                ReceiverAccountId = toAccount.Id,
                SenderAccountId = fromAccount.Id,
                Amount = amount,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Transactions.Add(transaction);

            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<List<TransactionHistoryResponse>>> GetTransactionHistoryAsync(int userId, DateTime? dateFrom, DateTime? dateTo, int skip, int take)
        {
            if(dateFrom != null && dateTo != null && dateFrom > dateTo)
            {
                return Result<List<TransactionHistoryResponse>>.Failure("Некорректный диапазон дат");
            }

            var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);


            var transactions = _dbContext.Transactions.Where(x => x.SenderAccountId == account!.Id || x.ReceiverAccountId == account.Id);

            if (dateFrom != null) 
            { 
                transactions = transactions.Where(x => x.CreatedAt >= dateFrom.Value);
            }
            if (dateTo != null) 
            { 
                transactions = transactions.Where(x => x.CreatedAt <= dateTo.Value);
            }
            transactions = transactions.Skip(skip).Take(take);

            var dbTransactions = await transactions.ToListAsync();

            var result = new List<TransactionHistoryResponse>();
            var allSenders = transactions.Select(x => x.SenderAccountId);
            var allReceivers = transactions.Select(x => x.ReceiverAccountId);
            var allAccounts = allSenders.ToHashSet();
            foreach (var receiver in allReceivers)
            {
                allAccounts.Add(receiver);
            }

            var names = await _dbContext.Accounts.Where(x => allAccounts.Contains(x.Id))
                .Join(_dbContext.Users,
                acc => acc.UserId,
                u => u.Id,
                (acc, u) => new 
                {
                    Name = u.Name,
                    AccId = acc.Id,
                }).ToDictionaryAsync(x => x.AccId);

            foreach (var transaction in transactions)
            {
                var senderName = names[transaction.SenderAccountId].Name;
                var receiverName = names[transaction.ReceiverAccountId].Name;
                result.Add(new TransactionHistoryResponse 
                { 
                    SenderName = senderName,
                    ReceiverName = receiverName,
                    Amount = transaction.Amount,
                    Date = transaction.CreatedAt
                });
            }

            return result;
        }
    }
}
