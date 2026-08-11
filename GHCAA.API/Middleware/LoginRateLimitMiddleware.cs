using System.Text.Json;

namespace GHCAA.API.Middleware
{
    // Reads the `username` field from the login request body on POST /api/auth/login
    // and stores it in HttpContext.Items so the PartitionedRateLimiter can key on (IP, username).
    // Must be registered BEFORE UseRateLimiter in the pipeline.
    public class LoginRateLimitMiddleware
    {
        private const string LoginPath = "/api/auth/login";
        private readonly RequestDelegate _next;

        public LoginRateLimitMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method)
                && context.Request.Path.StartsWithSegments(LoginPath, StringComparison.OrdinalIgnoreCase))
            {
                context.Request.EnableBuffering();
                try
                {
                    var doc = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: context.RequestAborted);
                    if (doc.RootElement.TryGetProperty("username", out var usernameProp))
                        context.Items["LoginUsername"] = usernameProp.GetString() ?? "";
                }
                catch { /* malformed body — rate-limit by IP only */ }
                finally
                {
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }
    }
}
