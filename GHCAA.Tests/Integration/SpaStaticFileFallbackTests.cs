using FluentAssertions;
using GHCAA.API.Extensions;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace GHCAA.Tests.Integration
{
    // Regression coverage for the 2026-08-28 preprod outage: MapFallback("/{**path}") without the
    // ":nonfile" route constraint matches every request — including real static assets — during
    // endpoint routing, which runs before UseStaticFiles gets a turn. StaticFileMiddleware backs off
    // whenever an endpoint is already matched, so ALL static files (main.js, styles.css, even
    // index.html itself) silently 404'd in production while working fine locally via `ng serve`.
    // See docs/TODO.md Work Package 44 / GHCAA.API/Program.cs for the fix: :nonfile restored on the SPA
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

        // Regression coverage for the EventPhotos/Members "broken image" console-noise fix: a missing
        // uploaded IMAGE (ephemeral Render disk wiped on redeploy, or bad seed/DB data) should render
        // silently via the shared placeholder instead of spamming the console with a 404, while a
        // missing non-image upload (e.g. a PDF certificate) must still 404 honestly above.
        [Test]
        public async Task MissingUploadImage_Returns200WithPlaceholder_NotSpammy404()
        {
            var response = await _client.GetAsync("/uploads/members/2/newsimage/does-not-exist.jpg");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("image/svg+xml");
            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("placeholder");
        }

        [Test]
        public async Task UnknownApiRoute_Returns404_NotSpaShell()
        {
            var response = await _client.GetAsync("/api/this-route-does-not-exist");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // NFR-P3. A repeat visit should not re-check build chunks whose names change with their content (98.6).
        [Test]
        public async Task HashedBuildChunk_IsCachedForAYear()
        {
            var response = await _client.GetAsync("/main-TESTHASH.js");

            string.Join(", ", response.Headers.GetValues("Cache-Control")).Should().Be(StaticFilesExtensions.ImmutableCacheControl);
        }

        [Test]
        public async Task IndexHtml_IsNeverCached()
        {
            var direct = await _client.GetAsync("/index.html");
            var deepLink = await _client.GetAsync("/portal/members");

            // HttpClient re-orders the directives when it parses the header, so check each one.
            foreach (var cache in new[] { direct.Headers.CacheControl!, deepLink.Headers.CacheControl! })
            {
                cache.NoStore.Should().BeTrue();
                cache.NoCache.Should().BeTrue();
                cache.MustRevalidate.Should().BeTrue();
            }
        }

        [Test]
        public async Task ExistingUpload_GetsTheShortUploadCache()
        {
            var response = await _client.GetAsync("/api/uploads/sample/existing.txt");

            string.Join(", ", response.Headers.GetValues("Cache-Control")).Should().Be(StaticFilesExtensions.UploadCacheControl);
        }

        [Test]
        public async Task MissingUploadPlaceholder_MustBeRevalidated()
        {
            var response = await _client.GetAsync("/uploads/members/2/newsimage/does-not-exist.jpg");

            string.Join(", ", response.Headers.GetValues("Cache-Control")).Should().Be(StaticFilesExtensions.RevalidateCacheControl);
        }

        // NFR-R5. The host's liveness probe must answer without touching the database (98.10).
        [Test]
        public async Task LivenessProbe_AnswersHealthy_NotSpaShell()
        {
            var response = await _client.GetAsync(Constants.Deploy.LivenessPath);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
        }

        // NFR-R4. /health is the full report from HealthController. The bare MapHealthChecks
        // endpoint that used to sit on the same path would make the route ambiguous.
        [Test]
        public async Task HealthPath_ServesTheFullReport()
        {
            var response = await _client.GetAsync(Constants.Deploy.HealthPath);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
            (await response.Content.ReadAsStringAsync()).Should().Contain("\"checks\"");
        }

        [TestCase("main-ABCD1234.js", true)]
        [TestCase("styles-EDLDB46M.css", true)]
        [TestCase("chunk-4AX5YQCX.js", true)]
        [TestCase("index.html", false)]
        [TestCase("favicon-32.png", false)]
        [TestCase("quill.min.js", false)]
        [TestCase("main-abcd1234.js", false)]
        public void IsHashedBuildFile_MatchesOnlyAngularHashedNames(string name, bool expected)
        {
            StaticFilesExtensions.IsHashedBuildFile(name).Should().Be(expected);
        }
    }
}
