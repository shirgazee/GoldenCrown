using GoldenCrown.Attributes;
using GoldenCrown.Database;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
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
