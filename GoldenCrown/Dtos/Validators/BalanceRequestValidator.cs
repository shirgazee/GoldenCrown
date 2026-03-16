using FluentValidation;
using GoldenCrown.Dtos.Finance;
using GoldenCrown.Models;

namespace GoldenCrown.Dtos.Validators
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
