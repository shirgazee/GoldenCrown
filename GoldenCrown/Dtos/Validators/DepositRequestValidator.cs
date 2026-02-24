using FluentValidation;
using GoldenCrown.Dtos.Finance;
using GoldenCrown.Dtos.User;

namespace GoldenCrown.Dtos.Validators
{
    public class DepositRequestValidator : AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");
        }
    }
}
