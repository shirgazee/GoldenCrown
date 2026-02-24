using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.Dtos.User
{
    public class RegisterRequest
    {
        public string Login { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}
