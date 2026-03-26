using FluentValidation;
using GoldenCrown.API.Dtos;
using GoldenCrown.Application.Dtos.Finance;
using GoldenCrown.Domain.Models;

namespace GoldenCrown.API.Validators
{
    public class BalanceRequestValidator : AbstractValidator<BalanceRequest>
    {
        public BalanceRequestValidator()
        {
            RuleFor(x => x.Currency)    
                .NotEmpty()
                .Must(currency => (new List<string>() {Currency.USD, Currency.EUR, Currency.GBP}).Contains(currency))
                .WithMessage("Укажите валюту");
        }
    }
}
