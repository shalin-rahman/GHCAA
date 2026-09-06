using GHCAA.Application.Security;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GHCAA.API.Middleware
{
    // 82.9: the only way to tie a client-reported error back to server log lines was to grep
    // Render output around a rough timestamp. This assigns one id per request (reusing a
    // caller-supplied X-Correlation-Id if it sent one, so a client that already tracks its own
    // request ids keeps the same value end to end), puts it on the response header, and wraps
    // the rest of the pipeline in a logger scope so every log line written while handling this
    // request — including ExceptionMiddleware's unhandled-exception line — carries it.
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue(Constants.Headers.CorrelationId, out var incoming)
                && !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : Guid.NewGuid().ToString();

            context.Items[Constants.Headers.CorrelationId] = correlationId;

            // Set before the response starts writing so it reaches the client even on an
            // unhandled-exception path (ExceptionMiddleware writes a fresh response but never
            // clears headers already set here).
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[Constants.Headers.CorrelationId] = correlationId;
                return Task.CompletedTask;
            });

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                var started = DateTime.UtcNow;
                await _next(context);
                var elapsedMs = (DateTime.UtcNow - started).TotalMilliseconds;

                // Read after _next() so authentication (which runs downstream of this
                // middleware) has already populated context.User for the request.
                var actingMemberId = context.User.FindFirst(AppClaimTypes.MemberId)?.Value ?? "anonymous";

                // One structured line per request, after the fact, so it carries the real
                // status code — a request log written up front can only ever guess that.
                _logger.LogInformation(
                    "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms (member {MemberId}, correlation {CorrelationId})",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    actingMemberId,
                    correlationId);
            }
        }
    }
}
