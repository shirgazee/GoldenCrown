using FluentValidation;
using GoldenCrown.API.Dtos;
using GoldenCrown.Application.Dtos.User;

namespace GoldenCrown.API.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Поле login обязательно")
                .MinimumLength(3).WithMessage("Минимальная длина логина от 3 символов");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Поле password обязательно")
                .MinimumLength(6).WithMessage("Минимальная длина пароля от 6 символов");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Поле name обязательно");
        }
    }
}
