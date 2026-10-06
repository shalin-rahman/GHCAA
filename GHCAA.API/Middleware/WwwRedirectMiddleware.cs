using System.Net;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace GHCAA.API.Middleware
{
    // 7.20. A request to www.<ClientUrl host> is moved to the same path and query on the
    // ClientUrl origin: a 301 for GET and HEAD, a 308 for anything else so a POST stays a POST. The host only ever comes from AppSettings:ClientUrl. The Render
    // custom-domain redirect stays in place too, so this is the fallback if that rule is lost.
    public class WwwRedirectMiddleware
    {
        private const string WwwPrefix = "www.";

        private readonly RequestDelegate _next;
        private readonly Uri? _canonical;

        public WwwRedirectMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _canonical = CanonicalOrigin(configuration[Constants.ConfigKeys.ClientUrl]);
        }

        public Task InvokeAsync(HttpContext context)
        {
            var target = RedirectTarget(_canonical, context.Request);
            if (target is null)
                return _next(context);

            context.Response.StatusCode = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)
                ? StatusCodes.Status301MovedPermanently
                : StatusCodes.Status308PermanentRedirect;
            context.Response.Headers.Location = target;
            return Task.CompletedTask;
        }

        // Null when there is nothing to redirect to: no ClientUrl, a localhost or IP address, or a
        // ClientUrl that is itself on www.
        public static Uri? CanonicalOrigin(string? clientUrl)
        {
            if (!Uri.TryCreate(clientUrl, UriKind.Absolute, out var uri))
                return null;
            if (uri.IsLoopback || IPAddress.TryParse(uri.Host, out _))
                return null;
            if (uri.Host.StartsWith(WwwPrefix, StringComparison.OrdinalIgnoreCase))
                return null;
            return uri;
        }

        public static string? RedirectTarget(Uri? canonical, HttpRequest request)
        {
            if (canonical is null)
                return null;
            if (!string.Equals(request.Host.Host, WwwPrefix + canonical.Host, StringComparison.OrdinalIgnoreCase))
                return null;
            var host = canonical.IsDefaultPort ? new HostString(canonical.Host) : new HostString(canonical.Host, canonical.Port);
            return UriHelper.BuildAbsolute(canonical.Scheme, host, request.PathBase, request.Path, request.QueryString);
        }
    }
}
