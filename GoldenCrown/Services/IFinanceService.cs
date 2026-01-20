using GoldenCrown.Dtos.Finance;

namespace GoldenCrown.Services
{
    public interface IFinanceService
    {
        Task<Result> DepositAsync(int userId, decimal amount);
        Task<Result<decimal>> GetBalanceAsync(int userId);
        Task<Result<List<TransactionHistoryResponse>>> GetTransactionHistoryAsync(int userId, DateTime? dateFrom, DateTime? dateTo, int skip, int take);
        Task<Result> TransferAsync(int fromUserId, string toLogin, decimal amount);
    }
}
