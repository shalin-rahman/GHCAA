using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace GHCAA.Tests.Integration
{
    // Regression coverage for the 2026-08-28 preprod outage: MapFallback("/{**path}") without the
    // ":nonfile" route constraint matches every request — including real static assets — during
    // endpoint routing, which runs before UseStaticFiles gets a turn. StaticFileMiddleware backs off
    // whenever an endpoint is already matched, so ALL static files (main.js, styles.css, even
    // index.html itself) silently 404'd in production while working fine locally via `ng serve`.
    // See docs/TODO.md Area 44 / GHCAA.API/Program.cs for the fix: :nonfile restored on the SPA
    // fallback route, with the "missing file -> 404 not 401" case moved to plain middleware instead
    // of a routed endpoint.
    [TestFixture]
    internal class SpaStaticFileFallbackTests
    {
        private SpaStaticFileFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new SpaStaticFileFactory();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task ExistingRootStaticFile_IsServedByStaticFiles_NotSwallowedByFallback()
        {
            var response = await _client.GetAsync("/main-TESTHASH.js");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("app bundle");
        }

        [Test]
        public async Task IndexHtml_RequestedDirectly_IsServedByStaticFiles()
        {
            var response = await _client.GetAsync("/index.html");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task DeepLink_WithNoFileExtension_FallsBackToSpaShell()
        {
            var response = await _client.GetAsync("/portal/members");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("spa-shell");
        }

        [Test]
        public async Task MissingRootStaticFile_Returns404_NotSpaShell()
        {
            var response = await _client.GetAsync("/chunk-DOES-NOT-EXIST.js");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task ExistingUpload_IsServedByStaticFiles()
        {
            var response = await _client.GetAsync("/api/uploads/sample/existing.txt");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("present");
        }

        [Test]
        public async Task MissingUpload_Returns404_NotUnauthorized()
        {
            var response = await _client.GetAsync("/api/uploads/sample/missing.txt");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UnknownApiRoute_Returns404_NotSpaShell()
        {
            var response = await _client.GetAsync("/api/this-route-does-not-exist");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
