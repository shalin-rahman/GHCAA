using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

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
        private readonly IMemoryCache _cache;

        public SecurityStampMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var stampClaim = context.User.FindFirstValue(Constants.SecurityStamps.ClaimType);

                if (int.TryParse(userIdClaim, out int userId) && !string.IsNullOrEmpty(stampClaim))
                {
                    // 98.8. One query per user every 30 seconds instead of one per request.
                    // SecurityStampCacheInterceptor drops the entry when a save rotates the stamp.
                    var dbStamp = await _cache.GetOrCreateAsync(Constants.SecurityStamps.CacheKey(userId), entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Constants.SecurityStamps.CacheSeconds);
                        return db.Users
                            .Where(u => u.Id == userId)
                            .Select(u => u.SecurityStamp)
                            .FirstOrDefaultAsync();
                    });

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
