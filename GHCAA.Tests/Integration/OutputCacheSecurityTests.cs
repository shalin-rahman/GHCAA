using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GHCAA.Tests.Integration
{
    // Regression coverage for the output-cache fix in GHCAA.API/Program.cs (docs/TODO.md Work
    // Package 80): the old base policy, AddOutputCache(o => o.AddBasePolicy(b => b.Cache())),
    // caches every GET/HEAD 200 response by URL alone. Output Cache doesn't vary by Cookie or
    // Authorization unless a policy says to, so a fixed-URL authenticated route like
    // /api/me/profile would serve one member's cached response to the next member who hit the
    // same URL inside the cache window. These tests run the real pipeline end to end: a direct
    // database write stands in for "the underlying data changed", and the next HTTP call proves
    // whether the response came from the database or from a stale cache entry.
    [TestFixture]
    internal class OutputCacheSecurityTests
    {
        // VisualTestAuthMiddleware.cs is the only place these token strings are defined; kept
        // as named constants here so a rename there has one obvious place to fix in this file.
        private const string MemberVisualToken = "visual_test_token";  // -> Member 200
        private const string AdminVisualToken = "visual_admin_token";  // -> SuperAdmin, Member 1
        private const int MemberSeedId = 200;
        private const int AdminSeedId = 1;
        private const string MemberFullNameFragment = "Shamsul";      // Member 200's name, Visual seed
        private const string AdminFullNameFragment = "Demo Member";   // Member 1's name, Visual seed

        private const string MeProfileRoute = "/api/me/profile";
        private const string LookupsRoute = "/api/lookups";
        private const int KnownLookupId = 1;
        private const string KnownLookupLabel = "HSC"; // Lookup 1's Label, Visual seed

        private OutputCacheTestFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new OutputCacheTestFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static HttpRequestMessage MeProfileRequest(string visualToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, MeProfileRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", visualToken);
            return request;
        }

        [Test]
        public async Task MeProfile_IsNotOutputCached_ReflectsAChangeMadeBetweenCalls()
        {
            var first = await _client.SendAsync(MeProfileRequest(MemberVisualToken));
            first.StatusCode.Should().Be(HttpStatusCode.OK);

            const string updatedName = "Cache Regression Marker";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var member = await db.Members.FindAsync(MemberSeedId);
                member!.FullName = updatedName;
                await db.SaveChangesAsync();
            }

            var second = await _client.SendAsync(MeProfileRequest(MemberVisualToken));
            var secondBody = await second.Content.ReadAsStringAsync();

            // A cached (buggy) response would still show the member's original name. A real
            // per-request read shows the write that just happened.
            secondBody.Should().Contain(updatedName);
        }

        [Test]
        public async Task MeProfile_AtTheSameUrl_NeverServesOneMembersDataToAnother()
        {
            var memberResponse = await _client.SendAsync(MeProfileRequest(MemberVisualToken));
            var adminResponse = await _client.SendAsync(MeProfileRequest(AdminVisualToken));

            var memberBody = await memberResponse.Content.ReadAsStringAsync();
            var adminBody = await adminResponse.Content.ReadAsStringAsync();

            memberBody.Should().NotBe(adminBody);
            memberBody.Should().Contain(MemberFullNameFragment);
            adminBody.Should().Contain(AdminFullNameFragment);
        }

        [Test]
        public async Task PublicLookups_IsOutputCached_ServesAStaleCopyWithinTheCacheWindow()
        {
            var first = await _client.GetAsync(LookupsRoute);
            var firstBody = await first.Content.ReadAsStringAsync();
            firstBody.Should().Contain(KnownLookupLabel);

            const string updatedLabel = "Cache Positive-Control Label";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var lookup = await db.Lookups.FindAsync(KnownLookupId);
                lookup!.Label = updatedLabel;
                await db.SaveChangesAsync();
            }

            var second = await _client.GetAsync(LookupsRoute);
            var secondBody = await second.Content.ReadAsStringAsync();

            // [OutputCache(PolicyName = PublicReference)] is doing its job when the response
            // still reflects the pre-write state a moment later — the opposite of the two
            // tests above, and proof the attribute is actually wired up, not just present.
            secondBody.Should().Contain(KnownLookupLabel);
            secondBody.Should().NotContain(updatedLabel);
        }
    }
}
