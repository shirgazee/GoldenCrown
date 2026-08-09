using System.Text.Json;
using GoldenCrown.Application.Events;
using GoldenCrown.Application.Services.Currency;
using GoldenCrown.Database;
using GoldenCrown.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Application.Features.Finance.Transfer
{
    public class TransferCommandHandler : IRequestHandler<TransferCommand, Result>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public TransferCommandHandler(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        public async Task<Result> Handle(TransferCommand request, CancellationToken cancellationToken)
        {
            var fromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.FromUserId && a.Currency == request.Currency, cancellationToken);
            if (fromAccount == null)
            {
                return Result.Failure("Счет отправителя не найден");
            }
            if (fromAccount!.Balance < request.Amount)
            {
                return Result.Failure("Недостаточно средств");
            }
            
            var toUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.ToLogin, cancellationToken);
            if (toUser == null)
            {
                return Result.Failure("Получатель не найден");
            }
            var toAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == toUser.Id && a.Currency == request.ReceiverCurrency, cancellationToken);
            if (toAccount == null)
            {
                return Result.Failure("Счет получателя не найден");
            }
            
            fromAccount.Balance -= request.Amount;
            var targetAmount = await _currencyService.Convert(request.Amount, fromAccount.Currency, toAccount.Currency, cancellationToken);
            toAccount!.Balance += targetAmount;
            var now = DateTime.UtcNow;
            
            var transaction = new Transaction
            {
                ReceiverAccountId = toAccount.Id,
                SenderAccountId = fromAccount.Id,
                Amount = request.Amount,
                CreatedAt = now,
                Currency =  request.Currency
            };
            _context.Transactions.Add(transaction);

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(TransactionCreatedEvent),
                Payload = JsonSerializer.Serialize(new TransactionCreatedEvent
                {
                    SenderId = request.FromUserId, 
                    ReceiverId = toUser.Id,
                    Amount = transaction.Amount,
                    Currency =  transaction.Currency
                }),
                CreatedAt = now,
                Attempts = 0
            });
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
    }
}
