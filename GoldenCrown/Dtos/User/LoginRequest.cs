using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.Dtos.User
{
    public class LoginRequest
    {
        public string Login { get; set; }
        
        public string Password { get; set; }
    }
}
