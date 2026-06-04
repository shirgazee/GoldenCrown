using GoldenCrown.Application.Events;
using GoldenCrown.Application.Services.Currency;
using GoldenCrown.Database;
using GoldenCrown.Domain.Models;
using GoldenCrown.Infrastructure.RabbitMQ;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Application.Features.Finance.Transfer
{
    public class TransferCommandHandler : IRequestHandler<TransferCommand, Result>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMessageProducer _messageProducer;
        private readonly ICurrencyService _currencyService;

        public TransferCommandHandler(ApplicationDbContext context, IMessageProducer messageProducer, ICurrencyService currencyService)
        {
            _context = context;
            _messageProducer = messageProducer;
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

            var transaction = new Transaction
            {
                ReceiverAccountId = toAccount.Id,
                SenderAccountId = fromAccount.Id,
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow,
                Currency =  request.Currency
            };
            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync(cancellationToken);
            
            await _messageProducer.SendMessageAsync(new TransactionCreatedEvent
            {
                SenderId = request.FromUserId, 
                ReceiverId = toUser.Id,
                Amount = transaction.Amount,
                Currency =  transaction.Currency
            }, cancellationToken);
            
            return Result.Success();
        }
    }
}
