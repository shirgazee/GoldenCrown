using GoldenCrown.Database;
using GoldenCrown.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public UserService(ApplicationDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public async Task<bool> RegisterAsync(string login, string name, string password)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
            if (existing != null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6) 
            {
                return false;
            }
            var user = new User
            {
                Login = login,
                Name = name,
                Password = password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _accountService.CreateAccountAsync(login);

            return true;
        }
    }
}
