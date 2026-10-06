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
    // TODO 7.20.
    [TestFixture]
    public class WwwRedirectMiddlewareTests
    {
        private static async Task<(HttpContext Context, bool CalledNext)> SendAsync(string? clientUrl, string host, string path = "/", string query = "", string method = "GET")
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [Constants.ConfigKeys.ClientUrl] = clientUrl })
                .Build();
            var calledNext = false;
            var middleware = new WwwRedirectMiddleware(_ => { calledNext = true; return Task.CompletedTask; }, configuration);
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Scheme = "https";
            context.Request.Host = new HostString(host);
            context.Request.Path = path;
            context.Request.QueryString = new QueryString(query);

            await middleware.InvokeAsync(context);

            return (context, calledNext);
        }

        [Test]
        public async Task WwwHost_IsMovedToTheClientUrlHost_WithPathAndQuery()
        {
            var (context, calledNext) = await SendAsync("https://example.org", "www.example.org", "/portal/elections", "?tab=2");

            calledNext.Should().BeFalse();
            context.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
            context.Response.Headers.Location.ToString().Should().Be("https://example.org/portal/elections?tab=2");
        }

        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("DELETE")]
        public async Task NonGetMethod_Gets308_SoTheMethodAndBodyAreKept(string method)
        {
            var (context, calledNext) = await SendAsync("https://example.org", "www.example.org", "/api/auth/login", method: method);

            calledNext.Should().BeFalse();
            context.Response.StatusCode.Should().Be(StatusCodes.Status308PermanentRedirect);
            context.Response.Headers.Location.ToString().Should().Be("https://example.org/api/auth/login");
        }

        [Test]
        public async Task Head_Gets301()
        {
            var (context, _) = await SendAsync("https://example.org", "www.example.org", method: "HEAD");

            context.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
        }

        [Test]
        public async Task HostMatch_IgnoresCase()
        {
            var (context, _) = await SendAsync("https://example.org", "WWW.Example.ORG");

            context.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
        }

        [Test]
        public async Task NonDefaultPortOnClientUrl_IsKept()
        {
            var (context, _) = await SendAsync("https://example.org:8443", "www.example.org");

            context.Response.Headers.Location.ToString().Should().Be("https://example.org:8443/");
        }

        [TestCase("https://example.org", "example.org")]
        [TestCase("https://example.org", "api.example.org")]
        [TestCase("https://example.org", "www.other.org")]
        [TestCase("http://localhost:4200", "www.localhost")]
        [TestCase("https://www.example.org", "www.example.org")]
        [TestCase("https://127.0.0.1", "www.127.0.0.1")]
        [TestCase(null, "www.example.org")]
        [TestCase("not a url", "www.example.org")]
        public async Task OtherRequests_PassThrough(string? clientUrl, string host)
        {
            var (context, calledNext) = await SendAsync(clientUrl, host);

            calledNext.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}
