using FluentValidation;
using GoldenCrown.Dtos.Finance;

namespace GoldenCrown.Dtos.Validators
{
    public class TransactionHistoryRequestValidator : AbstractValidator<TransactionHistoryRequest>
    {
        public TransactionHistoryRequestValidator()
        {
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Значение limit должно быть не меньше 1");
            RuleFor(x => x.Offset)
                .GreaterThanOrEqualTo(0).WithMessage("Значение offset не может быть отрицательным");
        }
    }
}
