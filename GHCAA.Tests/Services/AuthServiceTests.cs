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
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test1@e.com", NID = "123", FatherName="F", MotherName="M", MobileNo="01", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
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
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };
            var result = await _service.LoginAsync(loginDto);
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
        {
            var username = "testuser";
            var password = "CorrectPassword";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test2@e.com", NID = "124", FatherName="F", MotherName="M", MobileNo="01a", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            await _context.Users.AddAsync(new User { Username = username, PasswordHash = passwordHash, MemberId = member.Id, CreatedAt = DateTime.UtcNow, IsActive = true });
            await _context.SaveChangesAsync();

            var result = await _service.LoginAsync(new LoginDto { Username = username, Password = "WrongPassword" });
            result.Should().BeNull();
        }

        [Test]
        public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
        {
            var username = "inactiveuser";
            var password = "password";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var member = new Member { FullName = "Active Member", Status = Enums.MembershipStatus.Active, GHCLastCertificatePassingYear = 2007, Email = "test3@e.com", NID = "125", FatherName="F", MotherName="M", MobileNo="01b", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            await _context.Users.AddAsync(new User { Username = username, PasswordHash = passwordHash, MemberId = member.Id, CreatedAt = DateTime.UtcNow, IsActive = false });
            await _context.SaveChangesAsync();

            var result = await _service.LoginAsync(new LoginDto { Username = username, Password = password });
            result.Should().BeNull();
        }

        [Test]
        public async Task ResetPasswordAsync_WithValidToken_ShouldChangePassword()
        {
            // Arrange
            var email = "User@Example.com";
            var token = "token123";
            var member = new Member { FullName = "Test", Email = email, NID = "333", MobileNo = "333", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username = "testuser_reset",
                MemberId = member.Id,
                PasswordHash = "old_hash",
                ResetToken = token,
                ResetTokenExpiry = DateTime.UtcNow.AddHours(1),
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act - Test case-insensitive email (passing lowercase instead of MixedCase)
            var result = await _service.ResetPasswordAsync("user@example.com", token, "NewPassword123");

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
            var member = new Member { FullName = "Test", Email = email, NID = "444", MobileNo = "444", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username = "expired_user",
                MemberId = member.Id,
                PasswordHash = "old_hash",
                ResetToken = token,
                ResetTokenExpiry = DateTime.UtcNow.AddHours(-1),
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ResetPasswordAsync(email, token, "NewPassword123");

            // Assert
            result.Should().BeFalse();
        }
    }
}
