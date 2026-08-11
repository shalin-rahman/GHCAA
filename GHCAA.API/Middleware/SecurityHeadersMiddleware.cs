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
            // index.html loads Font Awesome + Quill (CSS/JS/webfonts) from cdnjs.cloudflare.com,
            // each pinned with an SRI integrity hash, so the cdnjs origin is whitelisted in
            // script-src / style-src / font-src. Google Fonts stay whitelisted for the Outfit
            // font. Without cdnjs here the CSP silently blocks those assets in prod (icons and
            // editor styling disappear). It looks fine in local dev only because `ng serve` hosts
            // the SPA on a separate origin, so this API-set CSP header never gates its assets;
            // in the combined prod deploy the API serves index.html and the CSP does apply.
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' https://cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' https:; " +
                "frame-ancestors 'none'; " +
                "form-action 'self';");

            await _next(context);
        }
    }
}
