using GoldenCrown.Database;
using GoldenCrown.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.User.UserRegister
{
    public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, Result>
    {
        private readonly ApplicationDbContext _context;

        public UserRegisterCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(x => x.Login == request.Login, cancellationToken);
            if (existing != null)
            {
                return Result.Failure("User already exists.");
            }

            var user = new Models.User
            {
                Login = request.Login,
                Name = request.Name,
                Password = request.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var account = new Account
            {
                UserId = user.Id,
                Balance = 0,
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
