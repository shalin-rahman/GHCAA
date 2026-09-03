using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using GHCAA.Application.Security;

namespace GHCAA.API.Middleware
{
    // Registered only when IsDevelopment() && ASP_SEED_PROFILE == "Visual" (Program.cs, near
    // UseAuthentication) — never reachable in a real deployment.
    /// <summary>Backdoor for Playwright visual tests: authenticates requests carrying a 'visual_*_token'.</summary>
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
                    claims.Add(new Claim(AppClaimTypes.MemberId, "200"));
                }
                else if (token == "visual_admin_token" || token == "visual_superadmin_token")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "1"));
                    claims.Add(new Claim(ClaimTypes.Name, "superadmin"));
                    claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
                    claims.Add(new Claim(AppClaimTypes.MemberId, "1"));
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
