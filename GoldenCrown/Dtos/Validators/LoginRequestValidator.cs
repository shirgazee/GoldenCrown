using FluentValidation;
using GoldenCrown.Dtos.User;

namespace GoldenCrown.Dtos.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>  
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Поле login обязательно")
                .MinimumLength(3).WithMessage("Минимальная длина логина от 3 символов");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Поле password обязательно")
                .MinimumLength(6).WithMessage("Минимальная длина пароля от 6 символов");
        }
    }
}
