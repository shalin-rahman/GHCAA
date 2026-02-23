using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
    public class AuthServiceTests
    {
        private ApplicationDbContext _context = null!;
        private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private Mock<ILogger<AuthService>> _mockLogger = null!;
        private Mock<IActivityService> _mockActivityService = null!;
        private AuthService _service = null!;

        [SetUp]
        public void Setup()
        {
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockActivityService = new Mock<IActivityService>();
            _service = new AuthService(_context, _mockTokenService.Object, _mockLogger.Object, _mockActivityService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenResponse()
        {
            // Arrange
            var username = "GHC-2007-0001";
            var password = "TestPassword123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test1@e.com", NID = "123", FatherName="F", MotherName="M", MobileNo="01", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            var memberId = member.Id;

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

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
            result.MemberId.Should().Be(memberId);
        }

        [Test]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
        {
            // Arrange
            var username = "testuser";
            var password = "CorrectPassword";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test2@e.com", NID = "124", FatherName="F", MotherName="M", MobileNo="01", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            var memberId = member.Id;

            await _context.Users.AddAsync(new User
            {
                Username = username,
                PasswordHash = passwordHash,
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Username = username, Password = "WrongPassword" };

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
        {
            // Arrange
            var username = "inactiveuser";
            var password = "password";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test3@e.com", NID = "125", FatherName="F", MotherName="M", MobileNo="01", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            var memberId = member.Id;

            await _context.Users.AddAsync(new User
            {
                Username = username,
                PasswordHash = passwordHash,
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            });
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Username = username, Password = password };

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }
    }
}
