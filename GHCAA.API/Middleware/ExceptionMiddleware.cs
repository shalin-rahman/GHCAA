using System.Net;
using System.Security.Claims;
using System.Text.Json;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GHCAA.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // IErrorLogService is resolved per-request (method injection) rather than through the
        // constructor because middleware is a singleton but the service and its DbContext are
        // scoped — see AuditLogMiddleware for the same pattern.
        public async Task InvokeAsync(HttpContext context, IErrorLogService errorLogService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

                // 45.3: this is awaited rather than truly fire-and-forget — errorLogService's
                // DbContext is request-scoped, and the scope is disposed the moment InvokeAsync
                // returns, so an un-awaited write would race that disposal. The try/catch around
                // it is defense in depth: IErrorLogService.LogAsync is contracted to never throw,
                // but if some future/mock implementation ever did, that must not replace the real
                // error already being handled with a logging failure instead.
                try
                {
                    await errorLogService.LogAsync(
                        Constants.ErrorLogs.LevelError,
                        ex.Message,
                        ex.GetType().FullName,
                        ex.StackTrace,
                        "ExceptionMiddleware",
                        context.Request.Path,
                        context.Request.Method,
                        TryGetUserId(context),
                        context.User?.Identity?.Name);
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Failed to persist ErrorLog for the exception above.");
                }

                // A throw after the response has already started writing (e.g. mid-SendFileAsync,
                // a streaming export, a compression flush) can't have its status/headers changed —
                // doing so anyway throws InvalidOperationException, which replaces the real error
                // above with a generic connection reset and hides what actually happened.
                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // 82.4: same ProblemDetails shape controllers return via Problem(...)/ValidationProblem(...)
                // — this path runs outside MVC's ProblemDetailsFactory, so it's built by hand here.
                var problem = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = "An unexpected error occurred.",
                    Detail = _env.IsDevelopment() ? ex.Message : "Internal Server Error",
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

                // 82.9: ties this error back to CorrelationIdMiddleware's request-scoped log lines.
                if (context.Items.TryGetValue(Constants.Headers.CorrelationId, out var correlationId) && correlationId is string cid)
                {
                    problem.Extensions["correlationId"] = cid;
                }
                if (_env.IsDevelopment())
                {
                    problem.Extensions["stackTrace"] = ex.StackTrace;
                }

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(problem, options);

                await context.Response.WriteAsync(json);
            }
        }

        private static int? TryGetUserId(HttpContext context)
        {
            var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
