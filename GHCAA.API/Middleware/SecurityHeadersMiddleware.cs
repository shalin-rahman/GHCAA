using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GHCAA.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Security Headers

            // 1. Prevent MIME-type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // 2. Clickjacking protection
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // 4. Strict-Transport-Security (HSTS) - Only for HTTPS
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            // 5. Referrer Policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // 6. Content Security Policy (CSP)
            // A basic restrictive CSP - can be tuned based on needs
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' https:; " +
                "frame-ancestors 'none'; " +
                "form-action 'self';");

            await _next(context);
        }
    }
}
