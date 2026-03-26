using FluentValidation;
using GoldenCrown.API.Dtos;
using GoldenCrown.Application.Dtos.Finance;

namespace GoldenCrown.API.Validators
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
