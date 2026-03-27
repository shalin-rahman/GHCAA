using System.Security.Claims;
using System.Text;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GHCAA.API.Middleware
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IActivityService activityService)
        {
            var request = context.Request;

            // Only log non-GET requests or admin paths
            if (request.Method != "GET" || request.Path.Value?.Contains("/api/admin") == true)
            {
                var user = context.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = user.Identity?.Name ?? "Anonymous";

                _logger.LogInformation("HTTP {Method} {Path} requested by {User}", request.Method, request.Path, username);

                // Capture response details after execution
                await _next(context);

                var statusCode = context.Response.StatusCode;
                if (statusCode >= 200 && statusCode < 300 && request.Method != "GET")
                {
                    // Log to database for destructive operations
                    if (int.TryParse(userId, out int parsedUserId))
                    {
                        await activityService.LogActivityAsync(
                            null, 
                            "ApiAction", 
                            $"User {username} performed {request.Method} {request.Path} (Status: {statusCode})", 
                            parsedUserId,
                            source: "API"
                        );
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
