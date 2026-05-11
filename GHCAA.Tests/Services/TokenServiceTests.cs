using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _mockConfig = null!;
        private Mock<IHostEnvironment> _mockEnv = null!;
        private TokenService _service = null!;
        private ApplicationDbContext _context = null!;
        private SqliteConnection _connection = null!;
        private const string Key = "super_secret_key_that_is_at_least_32_characters_long_for_hs256";

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(x => x["Jwt:Key"]).Returns(Key);
            _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("GHCAA");
            _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("GHCAA");

            _mockEnv = new Mock<IHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

            // SQLite in-memory supports ExecuteUpdateAsync; plain InMemory provider does not.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _service = new TokenService(_mockConfig.Object, _mockEnv.Object, NullLogger<TokenService>.Instance, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        [Test]
        public void CreateToken_ShouldReturnValidJwtToken()
        {
            var user = new User { Id = 1, Username = "testuser", MemberId = 100 };

            var token = _service.CreateToken(user);

            token.Should().NotBeNullOrEmpty();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be("GHCAA");
            jwtToken.Audiences.Should().Contain("GHCAA");
            jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
            jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
            jwtToken.Claims.Should().Contain(c => c.Type == "MemberId" && c.Value == "100");
        }

        [Test]
        public void CreateToken_ShouldExpireInApproximately60Minutes()
        {
            var user = new User { Id = 1, Username = "testuser" };
            var before = DateTime.UtcNow;

            var token = _service.CreateToken(user);

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            jwtToken.ValidTo.Should().BeCloseTo(before.AddMinutes(60), TimeSpan.FromSeconds(10));
        }

        [Test]
        public void GenerateRefreshToken_ShouldReturnBase64String()
        {
            var token = _service.GenerateRefreshToken();

            token.Should().NotBeNullOrEmpty();
            var bytes = Convert.FromBase64String(token);
            bytes.Should().HaveCount(32);
        }

        [Test]
        public async Task StoreAndRotateRefreshToken_ShouldRotateCorrectly()
        {
            var user = new User { Username = "u1", PasswordHash = "ph", SecurityStamp = Guid.NewGuid().ToString("N"), CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var plaintext = _service.GenerateRefreshToken();
            await _service.StoreRefreshTokenAsync(user.Id, plaintext);

            // Act
            var result = await _service.RotateRefreshTokenAsync(plaintext);

            // Assert
            result.Should().NotBeNull();
            result!.Value.NewToken.Should().NotBe(plaintext);
            result.Value.UserId.Should().Be(user.Id);

            // Old token should be revoked.
            var oldHash = ComputeHash(plaintext);
            var revoked = _context.RefreshTokens.First(r => r.TokenHash == oldHash);
            revoked.IsRevoked.Should().BeTrue();
        }

        [Test]
        public async Task RotateRefreshToken_WithInvalidToken_ShouldReturnNull()
        {
            var result = await _service.RotateRefreshTokenAsync("bad-token");
            result.Should().BeNull();
        }

        [Test]
        public async Task RevokeAllRefreshTokens_ShouldMarkAllRevoked()
        {
            var user = new User { Username = "u1", PasswordHash = "ph", SecurityStamp = Guid.NewGuid().ToString("N"), CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _service.StoreRefreshTokenAsync(user.Id, _service.GenerateRefreshToken());
            await _service.StoreRefreshTokenAsync(user.Id, _service.GenerateRefreshToken());

            await _service.RevokeAllRefreshTokensAsync(user.Id);

            _context.RefreshTokens.All(r => r.IsRevoked).Should().BeTrue();
        }

        private static string ComputeHash(string token)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
