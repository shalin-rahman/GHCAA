using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GHCAA.API.Middleware
{
    /// <summary>
    /// Middleware that allows passing the JWT token in the query string (e.g., ?token=...)
    /// and automatically adds it to the Authorization header if missing. 
    /// This is essential for opening PDF/Media files in new browser tabs where 
    /// the Bearer header cannot be manually injected (direct navigation).
    /// </summary>
    public class QueryStringTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public QueryStringTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If Authorization header is missing, check the query string
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                if (context.Request.Query.TryGetValue("token", out var token))
                {
                    context.Request.Headers.Append("Authorization", "Bearer " + token);
                }
                else if (context.Request.Query.TryGetValue("access_token", out var accessToken))
                {
                    context.Request.Headers.Append("Authorization", "Bearer " + accessToken);
                }
            }

            await _next(context);
        }
    }
}
