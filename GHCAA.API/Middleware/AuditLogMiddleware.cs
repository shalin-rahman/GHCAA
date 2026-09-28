using System.Security.Claims;
using System.Text;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
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

            if (IsVoteEndpoint(context))
            {
                await LogVoteAsync(context, activityService);
                return;
            }

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

        internal static bool IsVoteEndpoint(HttpContext context)
        {
            var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            return action?.ControllerTypeInfo.AsType() == typeof(ElectionsController)
                && action.ActionName == nameof(ElectionsController.Vote);
        }

        // Spec 023 FR-004. The vote route gets no log line and an audit row with the date only. A
        // time of day next to the ballot batch times would narrow down which batch a voter was in.
        private async Task LogVoteAsync(HttpContext context, IActivityService activityService)
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            if (statusCode < 200 || statusCode >= 300 || !int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return;

            var username = context.User.Identity?.Name ?? "Anonymous";
            var electionId = context.Request.RouteValues["id"];
            await activityService.LogActivityAsync(
                null,
                Constants.Elections.VoteAuditType,
                $"User {username} voted in election {electionId}",
                userId,
                source: "API",
                timestamp: DateTime.UtcNow.Date);
        }
    }
}
