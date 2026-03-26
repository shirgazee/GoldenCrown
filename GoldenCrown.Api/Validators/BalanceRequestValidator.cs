using FluentValidation;
using GoldenCrown.Api.Dtos;
using GoldenCrown.Api.Dtos.Finance;
using GoldenCrown.Domain.Models;

namespace GoldenCrown.Api.Validators
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
