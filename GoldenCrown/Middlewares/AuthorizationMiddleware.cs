using GoldenCrown.Attributes;
using GoldenCrown.Database;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public AuthorizationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var attribute = context.GetEndpoint()?.Metadata.GetMetadata<MyAuthorizeAttribute>();
            if (attribute == null)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers[Constants.Authorization].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var session = await dbContext.Sessions.FirstOrDefaultAsync(x => x.Token == token);
            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.Items[Constants.UserIdContextParameter] = session.UserId;

            await _next(context);
        }
    }
}
