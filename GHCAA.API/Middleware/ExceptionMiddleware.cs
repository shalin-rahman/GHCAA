using System.Net;
using System.Text.Json;
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

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
    }
}
