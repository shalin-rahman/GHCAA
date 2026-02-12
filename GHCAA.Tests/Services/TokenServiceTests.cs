using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _mockConfig = null!;
        private TokenService _service = null!;
        private string _key = "super_secret_key_that_is_at_least_32_characters_long_for_hs256";

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(x => x["Jwt:Key"]).Returns(_key);
            _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("GHCAA");
            _mockConfig.Setup(x => x["Jwt:Audience"]).Returns("GHCAA");

            _service = new TokenService(_mockConfig.Object);
        }

        [Test]
        public void CreateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                MemberId = 100
            };

            // Act
            var token = _service.CreateToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be("GHCAA");
            jwtToken.Audiences.Should().Contain("GHCAA");
            
            jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
            jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
            jwtToken.Claims.Should().Contain(c => c.Type == "MemberId" && c.Value == "100");
        }
    }
}
