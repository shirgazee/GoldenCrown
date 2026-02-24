using FluentValidation;
using GoldenCrown.Dtos.Finance;

namespace GoldenCrown.Dtos.Validators
{
    public class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.ReceiverLogin)
                .NotEmpty().WithMessage("Укажите логин получателя");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");
        }
    }
}
