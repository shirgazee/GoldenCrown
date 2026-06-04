using FluentValidation;
using GoldenCrown.Api.Dtos.Finance;
using GoldenCrown.Domain.Models;

namespace GoldenCrown.Api.Validators
{
    public class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.ReceiverLogin)
                .NotEmpty().WithMessage("Укажите логин получателя");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");

            RuleFor(x => x.Currency)    
                .NotEmpty()
                .Must(currency => Currency.AllCurrencies.Contains(currency))
                .WithMessage("Укажите валюту");
            
            RuleFor(x => x.ReceiverCurrency)    
                .NotEmpty()
                .Must(currency => Currency.AllCurrencies.Contains(currency))
                .WithMessage("Укажите валюту получателя");
        }
    }
}
