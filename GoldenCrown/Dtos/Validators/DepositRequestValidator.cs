using FluentValidation;
using GoldenCrown.Dtos.Finance;
using GoldenCrown.Dtos.User;
using GoldenCrown.Models;

namespace GoldenCrown.Dtos.Validators
{
    public class DepositRequestValidator : AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");
            
            RuleFor(x => x.Currency)    
                .NotEmpty()
                .Must(currency => (new List<string>() {Currency.USD, Currency.EUR, Currency.GBP}).Contains(currency))
                .WithMessage("Укажите валюту");
        }
    }
}
