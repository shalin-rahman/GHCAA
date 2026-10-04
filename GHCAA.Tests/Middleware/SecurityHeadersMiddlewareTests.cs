using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.API.Middleware;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GHCAA.Tests.Middleware
{
    [TestFixture]
    public class SecurityHeadersMiddlewareTests
    {
        private static async Task<string> CspAsync(params string[] allowedOrigins)
        {
            var settings = new Dictionary<string, string?>();
            for (var i = 0; i < allowedOrigins.Length; i++)
                settings[$"{Constants.ConfigKeys.AllowedOrigins}:{i}"] = allowedOrigins[i];
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, configuration);
            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            return context.Response.Headers["Content-Security-Policy"].ToString();
        }

        // TODO 37.13k. Script in the page must not be able to send data to any https host.
        [Test]
        [Category("NFR-S1")]
        public async Task ConnectSrc_IsOwnOriginAndAllowedOriginsOnly()
        {
            var csp = await CspAsync("https://alumni.example.org", "http://localhost:4200/");

            csp.Should().Contain("connect-src 'self' https://alumni.example.org http://localhost:4200;");
            csp.Should().NotContain("connect-src 'self' https:;");
        }

        [Test]
        [Category("NFR-S1")]
        public async Task ConnectSrc_IsOwnOriginOnly_WhenNoOriginsAreSet()
        {
            (await CspAsync()).Should().Contain("connect-src 'self';");
        }

        [Test]
        [Category("NFR-S1")]
        public async Task ConnectSrc_SkipsValuesThatAreNotOrigins()
        {
            var csp = await CspAsync("https://ok.example.org/", "https://x;img-src@evil.example.org", "https://p.example.org/some/path",
                "https:; script-src *", "javascript:alert(1)", "");

            csp.Should().Contain("connect-src 'self' https://ok.example.org;");
            csp.Should().NotContain("script-src *").And.NotContain("evil").And.NotContain("p.example.org");
        }
    }
}
