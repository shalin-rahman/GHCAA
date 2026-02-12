using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private ApplicationDbContext _context = null!;
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private UserService _service = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<UserService>>();
            _service = new UserService(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateUserAccountAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var memberId = 1;
            var username = "GHC-2007-0001";
            var password = "TestPassword123";

            // Act
            var user = await _service.CreateUserAccountAsync(memberId, username, password);

            // Assert
            user.Should().NotBeNull();
            user.Username.Should().Be(username);
            user.MemberId.Should().Be(memberId);
            user.IsActive.Should().BeTrue();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().NotBe(password); // Should be hashed
            
            // Verify password hash is BCrypt
            BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateMemberId_ShouldThrowException()
        {
            // Arrange
            var memberId = 1;
            await _context.Users.AddAsync(new User
            {
                Username = "existing",
                PasswordHash = "hash",
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var act = async () => await _service.CreateUserAccountAsync(memberId, "newuser", "password");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User account already exists for member {memberId}");
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateUsername_ShouldThrowException()
        {
            // Arrange
            var username = "GHC-2007-0001";
            await _context.Users.AddAsync(new User
            {
                Username = username,
                PasswordHash = "hash",
                MemberId = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var act = async () => await _service.CreateUserAccountAsync(2, username, "password");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Username '{username}' is already taken");
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturn8CharacterPassword()
        {
            // Act
            var password = _service.GenerateDefaultPassword();

            // Assert
            password.Should().NotBeNullOrEmpty();
            password.Length.Should().Be(8);
            password.Should().MatchRegex("^[A-Za-z0-9]{8}$"); // Only alphanumeric
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturnDifferentPasswords()
        {
            // Act
            var password1 = _service.GenerateDefaultPassword();
            var password2 = _service.GenerateDefaultPassword();
            var password3 = _service.GenerateDefaultPassword();

            // Assert - At least one should be different (very high probability)
            var allSame = password1 == password2 && password2 == password3;
            allSame.Should().BeFalse();
        }

        [Test]
        public async Task CreateUserAccountAsync_ShouldHashPasswordWithBCrypt()
        {
            // Arrange
            var password = "MySecurePassword123";

            // Act
            var user = await _service.CreateUserAccountAsync(1, "testuser", password);

            // Assert
            user.PasswordHash.Should().StartWith("$2"); // BCrypt hash starts with $2a, $2b, etc.
            BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("WrongPassword", user.PasswordHash).Should().BeFalse();
        }
    }
}
