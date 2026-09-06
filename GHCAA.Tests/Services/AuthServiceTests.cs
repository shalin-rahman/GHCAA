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
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _mockConfig = null!;
        private Mock<System.Net.Http.IHttpClientFactory> _mockHttp = null!;
        private Mock<IEmailService> _mockEmail = null!;
        private Mock<ICommunicationService> _mockCommunicationService = null!;
        private Mock<IOrgConfigService> _mockOrgConfigService = null!;
        private AuthService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockActivityService = new Mock<IActivityService>();
            _mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _mockHttp = new Mock<System.Net.Http.IHttpClientFactory>();
            _mockEmail = new Mock<IEmailService>();
            _mockCommunicationService = new Mock<ICommunicationService>();
            _mockOrgConfigService = new Mock<IOrgConfigService>();
            _mockOrgConfigService.Setup(x => x.GetConfigAsync())
                .ReturnsAsync(new OrgConfigDto { Branding = new BrandingDto { ShortName = "GHCAA" } });
            _service = new AuthService(_context, _mockTokenService.Object, _mockLogger.Object, _mockActivityService.Object, _mockConfig.Object, _mockHttp.Object, _mockEmail.Object, _mockCommunicationService.Object, _mockOrgConfigService.Object);
        }

        [Category("FR-08")]
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

        [Category("FR-08")]
        [Test]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnNull()
        {
            var loginDto = new LoginDto { Username = "nonexistent", Password = "password" };
            var result = await _service.LoginAsync(loginDto);
            result.Should().BeNull();
        }

        [Category("FR-08")]
        [Test]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
        {
            var username = "testuser";
            var member = await CreateAndSaveTestMemberAsync("Active Member", "test2@e.com", "124", "124");
            await CreateAndSaveTestUserAsync(member.Id, username, "CorrectPassword");

            var result = await _service.LoginAsync(new LoginDto { Username = username, Password = "WrongPassword" });
            result.Should().BeNull();
        }

        [Category("FR-08")]
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

        [Category("FR-08")]
        [Test]
        public async Task LoginAsync_AfterFiveFailedAttempts_ShouldLockOutForFifteenMinutes()
        {
            var username = "lockout_user";
            var password = "ValidPass1!";
            var member = await CreateAndSaveTestMemberAsync("Lockout Member", "lockout@e.com", "126", "126");
            var user = await CreateAndSaveTestUserAsync(member.Id, username, password);

            for (var i = 0; i < 5; i++)
            {
                var fail = await _service.LoginAsync(new LoginDto { Username = username, Password = $"WrongPass{i}!" });
                fail.Should().BeNull();
            }

            var locked = await _service.LoginAsync(new LoginDto { Username = username, Password = password });
            locked.Should().BeNull();

            var updated = await _context.Users.FindAsync(user.Id);
            updated!.FailedLoginAttempts.Should().BeGreaterOrEqualTo(5);
            updated.LockoutUntil.Should().NotBeNull();
            updated.LockoutUntil!.Value.Should().BeAfter(DateTime.UtcNow);
        }

        [Category("FR-09")]
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

        [Category("FR-09")]
        [Test]
        public async Task RequestPasswordResetAsync_KnownEmail_GeneratesTokenAndSendsEmail()
        {
            var email = "resetme@example.com";
            var member = await CreateAndSaveTestMemberAsync("Reset Me", email, "444", "444");
            var user = await CreateAndSaveTestUserAsync(member.Id, "testuser_forgot", "old_password");

            await _service.RequestPasswordResetAsync(email);

            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.ResetToken.Should().NotBeNullOrEmpty();
            updatedUser.ResetTokenExpiry.Should().NotBeNull().And.BeAfter(DateTime.UtcNow);
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
            _mockEmail.Verify(x => x.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-09")]
        [Test]
        public async Task RequestPasswordResetAsync_UnknownIdentifier_DoesNothingObservableAndDoesNotThrow()
        {
            var act = async () => await _service.RequestPasswordResetAsync("nobody@example.com");

            await act.Should().NotThrowAsync();
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockEmail.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-09")]
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

        [Category("FR-09")]
        [Test]
        public async Task ResetPasswordAsync_ForSystemAdminByUsername_ShouldChangePassword()
        {
            // A system admin has no Member/email, so its reset link carries Username in the
            // Email field instead. No Member row matches, so the MemberId == null fallback path
            // must be the one that finds it.
            var username = "sysadmin_reset";
            var token = "sysadmin_token";
            var user = new User { Username = username, PasswordHash = "old", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true, ResetToken = token, ResetTokenExpiry = DateTime.UtcNow.AddHours(1) };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var result = await _service.ResetPasswordAsync(username, token, "NewPassword123");

            result.Should().BeTrue();
            var updatedUser = await _context.Users.FindAsync(user.Id);
            BCrypt.Net.BCrypt.Verify("NewPassword123", updatedUser!.PasswordHash).Should().BeTrue();
            updatedUser.ResetToken.Should().BeNull();
        }

        [Category("FR-09")]
        [Test]
        public async Task ResetPasswordAsync_MemberCannotBeResetByUsernameFallback()
        {
            // The MemberId == null fallback must not also let a member's account be reset by
            // guessing their Username — it is scoped to system-admin accounts only.
            var member = await CreateAndSaveTestMemberAsync("Test", "scoped@example.com", "555", "555");
            var user = await CreateAndSaveTestUserAsync(member.Id, "member_username_only");
            user.ResetToken = "member_token";
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var result = await _service.ResetPasswordAsync("member_username_only", "member_token", "NewPassword123");

            result.Should().BeFalse();
        }

        [Category("FR-10")]
        [Test]
        public async Task SocialLoginAsync_WithValidGoogleId_ShouldReturnTokenResponse()
        {
            // Arrange
            var email = "social@example.com";
            var googleId = "google_12345";
            var member = await CreateAndSaveTestMemberAsync("Social Member", email, "555", "555");
            var user = await CreateAndSaveTestUserAsync(member.Id, "social_user", "password");
            user.GoogleId = googleId;
            await _context.SaveChangesAsync();

            var expectedToken = "mock_social_jwt_token";
            _mockTokenService.Setup(x => x.CreateToken(It.IsAny<User>())).Returns(expectedToken);

            // Act
            var result = await _service.SocialLoginAsync(googleId, email, "Social Member", "Google");

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be(expectedToken);
            result.MemberId.Should().Be(member.Id);
        }

        // 80.13: extracted from AuthController's Refresh/RefreshMobile/step-up flows so it
        // doesn't touch ApplicationDbContext directly.
        [Test]
        public async Task GetUserWithRolesAsync_ReturnsUser_WhenFound()
        {
            var user = new User { Username = "u1", PasswordHash = "h", SecurityStamp = "s", CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserWithRolesAsync(user.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
        }

        [Test]
        public async Task GetUserWithRolesAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _service.GetUserWithRolesAsync(999);

            result.Should().BeNull();
        }

        [Test]
        public async Task GetUserWithRolesAndMemberAsync_IncludesMember()
        {
            var member = new Member
            {
                FullName = "M", FatherName = "F", MotherName = "Mo", Email = "m@example.com", NID = "N",
                MobileNo = "01700000000", PresentAddress = "A", PermanentAddress = "A",
                EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0"
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var user = new User { Username = "u2", PasswordHash = "h", SecurityStamp = "s", CreatedAt = DateTime.UtcNow, MemberId = member.Id };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserWithRolesAndMemberAsync(user.Id);

            result.Should().NotBeNull();
            result!.Member.Should().NotBeNull();
            result.Member!.Email.Should().Be("m@example.com");
        }

        [Test]
        public async Task GetUserByUsernameAsync_ReturnsMatch()
        {
            var user = new User { Username = "findme", PasswordHash = "h", SecurityStamp = "s", CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetUserByUsernameAsync("findme");

            result.Should().NotBeNull();
            result!.Username.Should().Be("findme");
        }

        [Test]
        public async Task GetUserByUsernameAsync_ReturnsNull_WhenNoMatch()
        {
            var result = await _service.GetUserByUsernameAsync("nobody");

            result.Should().BeNull();
        }
    }
}
