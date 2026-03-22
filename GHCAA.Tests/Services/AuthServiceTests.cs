using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests : TestBase
    {
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<ILogger<AuthService>> _mockLogger = null!;
        private Mock<IActivityService> _mockActivityService = null!;
        private AuthService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockActivityService = new Mock<IActivityService>();
            _service = new AuthService(_context, _mockTokenService.Object, _mockLogger.Object, _mockActivityService.Object);
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenResponse()
        {
            // Arrange
            var username = "GHC-2007-0001";
            var password = "TestPassword123";
            var member = await CreateAndSaveTestMemberAsync("Active Member", "test1@e.com", "123", "123");
            var user = await CreateAndSaveTestUserAsync(member.Id, username, password);

            var expectedToken = "mock_jwt_token";
            _mockTokenService.Setup(x => x.CreateToken(It.IsAny<User>()))
                .Returns(expectedToken);

            var loginDto = new LoginDto { Username = username, Password = password };

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be(expectedToken);
            result.Username.Should().Be(username);
            result.MemberId.Should().Be(member.Id);
        }

        [Test]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnNull()
        {
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };
            var result = await _service.LoginAsync(loginDto);
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
        {
            var username = "testuser";
            var member = await CreateAndSaveTestMemberAsync("Active Member", "test2@e.com", "124", "124");
            await CreateAndSaveTestUserAsync(member.Id, username, "CorrectPassword");

            var result = await _service.LoginAsync(new LoginDto { Username = username, Password = "WrongPassword" });
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
        {
            var username = "inactiveuser";
            var password = "password";
            var member = await CreateAndSaveTestMemberAsync("Active Member", "test3@e.com", "125", "125");
            var user = await CreateAndSaveTestUserAsync(member.Id, username, password);
            user.IsActive = false;
            await _context.SaveChangesAsync();

            var result = await _service.LoginAsync(new LoginDto { Username = username, Password = password });
            result.Should().BeNull();
        }

        [Test]
        public async Task ResetPasswordAsync_WithValidToken_ShouldChangePassword()
        {
            // Arrange
            var email = "user@example.com";
            var token = "token123";
            var member = await CreateAndSaveTestMemberAsync("Test", email, "333", "333");
            var user = await CreateAndSaveTestUserAsync(member.Id, "testuser_reset", "old_password");
            
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ResetPasswordAsync(email, token, "NewPassword123");

            // Assert
            result.Should().BeTrue();
            var updatedUser = await _context.Users.FindAsync(user.Id);
            BCrypt.Net.BCrypt.Verify("NewPassword123", updatedUser!.PasswordHash).Should().BeTrue();
            updatedUser.ResetToken.Should().BeNull();
        }

        [Test]
        public async Task ResetPasswordAsync_WithExpiredToken_ShouldReturnFalse()
        {
            // Arrange
            var email = "expired@example.com";
            var token = "expired_token";
            var member = await CreateAndSaveTestMemberAsync("Test", email, "444", "444");
            var user = await CreateAndSaveTestUserAsync(member.Id, "expired_user", "old_password");
            
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(-1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ResetPasswordAsync(email, token, "NewPassword123");

            // Assert
            result.Should().BeFalse();
        }
    }
}
