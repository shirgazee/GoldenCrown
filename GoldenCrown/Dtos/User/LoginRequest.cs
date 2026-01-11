using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.Dtos.User
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Поле login обязательно")]
        [MinLength(3, ErrorMessage = "Минимальная длина логина от 3 символов")]
        public string Login { get; set; }
        
        [Required(ErrorMessage = "Поле password обязательно")]
        [MinLength(6, ErrorMessage = "Минимальная длина пароля от 6 символов")]
        public string Password { get; set; }
    }
}
