using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GHCAA.API.Middleware
{
    // TODO [CRITICAL]: This middleware is registered unconditionally in Program.cs regardless of environment.
    // Anyone who knows "visual_test_token" / "visual_admin_token" / "visual_superadmin_token" gains full
    // admin access on any deployment. Wrap Program.cs registration in:
    //   if (env.IsDevelopment() && Configuration["ASP_SEED_PROFILE"] == "Visual")
    // Also reject any "Bearer visual_*" token outside the Visual profile to prevent accidental exposure.
    /// <summary>
    /// This middleware provides a backdoor for Playwright visual tests.
    /// It detects the 'visual_*_token' and automatically authenticates the request.
    /// </summary>
    public class VisualTestAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public VisualTestAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer visual_"))
            {
                var token = authHeader.Replace("Bearer ", "");
                var claims = new List<Claim>();

                if (token == "visual_test_token")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "200"));
                    claims.Add(new Claim(ClaimTypes.Name, "mdshamsulislam"));
                    claims.Add(new Claim(ClaimTypes.Role, "Member"));
                    claims.Add(new Claim("MemberId", "200"));
                }
                else if (token == "visual_admin_token" || token == "visual_superadmin_token")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
                    claims.Add(new Claim(ClaimTypes.Name, "superadmin"));
                    claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                    claims.Add(new Claim("MemberId", "1"));
                }

                if (claims.Count > 0)
                {
                    var identity = new ClaimsIdentity(claims, "VisualMock");
                    context.User = new ClaimsPrincipal(identity);
                }
            }

            await _next(context);
        }
    }
}
