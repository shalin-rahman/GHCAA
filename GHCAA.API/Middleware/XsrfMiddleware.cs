namespace GHCAA.API.Middleware
{
    /// <summary>
    /// Double-submit-cookie CSRF protection for cookie-authenticated (browser) clients.
    /// Angular's HttpClient reads the non-httpOnly XSRF-TOKEN cookie and echoes it back
    /// as the X-XSRF-TOKEN header on every request; a cross-site page cannot read the
    /// cookie to forge that header. Bearer/mobile clients never send the access_token
    /// cookie so they are unaffected.
    /// </summary>
    public class XsrfMiddleware
    {
        private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "GET", "HEAD", "OPTIONS", "TRACE"
        };

        private readonly RequestDelegate _next;

        public XsrfMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            if (SafeMethods.Contains(request.Method) ||
                !request.Cookies.ContainsKey("access_token"))
            {
                await _next(context);
                return;
            }

            var cookieToken = request.Cookies["XSRF-TOKEN"];
            var headerToken = request.Headers["X-XSRF-TOKEN"].ToString();

            if (string.IsNullOrEmpty(cookieToken) ||
                string.IsNullOrEmpty(headerToken) ||
                !string.Equals(cookieToken, headerToken, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "CSRF token missing or invalid." });
                return;
            }

            await _next(context);
        }
    }
}
