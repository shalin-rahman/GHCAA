using System.Security.Claims;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Middleware
{
    /// <summary>
    /// Validates that the SecurityStamp embedded in the JWT still matches the user's
    /// current stamp in the database. When an admin terminates or deactivates a member,
    /// the stamp is rotated, instantly invalidating all existing tokens without waiting
    /// for the JWT expiry window.
    /// </summary>
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var stampClaim = context.User.FindFirstValue("SecurityStamp");

                if (int.TryParse(userIdClaim, out int userId) && !string.IsNullOrEmpty(stampClaim))
                {
                    var dbStamp = await db.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.SecurityStamp)
                        .FirstOrDefaultAsync();

                    if (dbStamp != stampClaim)
                    {
                        // Stamp mismatch — token has been invalidated
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = "Session has been terminated. Please log in again."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
