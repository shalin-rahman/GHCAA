using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GHCAA.Domain;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace GHCAA.Tests.Integration
{
    // 84.45: the three response paths that used to send an empty body — 401 (no token), 403
    // (wrong role) and 429 (rate limited) — now write a ProblemDetails body with a "code"
    // extension. These drive the real pipeline end to end through the test host instead of
    // unit-testing the handlers in isolation, since the thing worth proving is that
    // JwtBearerEvents and the rate limiter are actually wired to ProblemExtensions.
    [TestFixture]
    public class ErrorResponseShapeTests
    {
        private ErrorResponseShapeTestFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new ErrorResponseShapeTestFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        [Category("NFR-S6")]
        public async Task AdminRoute_WithNoToken_Returns401WithUnauthenticatedCode()
        {
            var response = await _client.GetAsync("/api/admin/stats");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("code").GetString().Should().Be(Constants.ErrorCodes.Unauthenticated);
        }

        [Test]
        [Category("NFR-S6")]
        public async Task AdminRoute_WithMemberToken_Returns403WithForbiddenCode()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", BuildMemberToken());

            var response = await _client.GetAsync("/api/admin/stats");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("code").GetString().Should().Be(Constants.ErrorCodes.Forbidden);
        }

        [Test]
        [Category("NFR-S3")]
        public async Task LoginRoute_AfterExceedingAuthLimit_Returns429WithRateLimitedCode()
        {
            // Production env keeps the real Auth policy limit (10/min), so 11 bogus logins
            // is enough to trip it — no need for the Development 500/min ceiling.
            HttpResponseMessage? last = null;
            for (var i = 0; i < 11; i++)
            {
                last = await _client.PostAsJsonAsync("/api/auth/login", new { Username = "no-such-user", Password = "wrong-password" });
            }

            last!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
            last.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

            var body = await last.Content.ReadFromJsonAsync<JsonElement>();
            body.GetProperty("code").GetString().Should().Be(Constants.ErrorCodes.RateLimited);
        }

        private static string BuildMemberToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "error-shape-test-member"),
                new Claim(ClaimTypes.Role, Constants.Roles.Member),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ErrorResponseShapeTestFactory.JwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: ErrorResponseShapeTestFactory.JwtIssuer,
                audience: ErrorResponseShapeTestFactory.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
