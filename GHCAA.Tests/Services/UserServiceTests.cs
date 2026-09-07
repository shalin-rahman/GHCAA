using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class UserServiceTests : TestBase
    {
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private IOptions<AppSettingsOptions> _appSettings = null!;
        private UserService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockTokenService = new Mock<ITokenService>();
            _appSettings = Options.Create(new AppSettingsOptions());
            _service = new UserService(_context, _mockLogger.Object, _mockTokenService.Object, _appSettings);
        }

        [Test]
        public async Task CreateUserAccountAsync_WithValidData_ShouldCreateUser()
        {
            var member = new Member { FullName = "Test Member", Email = "valid@e.com", NID = "V1", MobileNo = "V1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "GHC-TEST-0001", "TestPassword123");

            user.Should().NotBeNull();
            user.MemberId.Should().Be(member.Id);
            user.IsActive.Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("TestPassword123", user.PasswordHash).Should().BeTrue();
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateMemberId_ShouldThrowException()
        {
            var member = new Member { FullName = "Test Member", Email = "dup@e.com", NID = "D1", MobileNo = "D1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            await _context.Users.AddAsync(new User { Username = "existing-dup", PasswordHash = "hash", MemberId = member.Id, CreatedAt = DateTime.UtcNow, IsActive = true });
            await _context.SaveChangesAsync();

            var act = async () => await _service.CreateUserAccountAsync(member.Id, "newuser-dup", "password");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"User account already exists for member {member.Id}");
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateUsername_ShouldThrowException()
        {
            var member1 = new Member { FullName = "Test Member 1", Email = "test1un@e.com", NID = "UN1", MobileNo = "UN1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            var member2 = new Member { FullName = "Test Member 2", Email = "test2un@e.com", NID = "UN2", MobileNo = "UN2", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddRangeAsync(member1, member2);
            await _context.SaveChangesAsync();

            var takenUsername = "GHC-TAKEN-DUP";
            await _context.Users.AddAsync(new User { Username = takenUsername, PasswordHash = "hash", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true });
            await _context.SaveChangesAsync();

            var act = async () => await _service.CreateUserAccountAsync(member2.Id, takenUsername, "password");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Username '{takenUsername}' is already taken");
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturn8CharacterPassword()
        {
            var password = _service.GenerateDefaultPassword();
            password.Should().NotBeNullOrEmpty();
            password.Length.Should().Be(8);
            password.Should().MatchRegex("^[A-Za-z0-9]{8}$");
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturnDifferentPasswords()
        {
            var p1 = _service.GenerateDefaultPassword();
            var p2 = _service.GenerateDefaultPassword();
            var p3 = _service.GenerateDefaultPassword();
            (p1 == p2 && p2 == p3).Should().BeFalse();
        }

        [Category("FR-08")]
        [Test]
        public async Task CreateUserAccountAsync_ShouldHashPasswordWithBCrypt()
        {
            var member = new Member { FullName = "BCrypt Test", Email = "bcrypt@e.com", NID = "BCRYPT1", MobileNo = "BCRYPT1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "hashed-user-test", "MySecurePassword123");

            user.PasswordHash.Should().StartWith("$2");
            BCrypt.Net.BCrypt.Verify("MySecurePassword123", user.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("WrongPassword", user.PasswordHash).Should().BeFalse();
        }

        [Test]
        public async Task ChangePasswordAsync_WithValidData_ShouldUpdatePassword()
        {
            var member = new Member { FullName = "Test Member", Email = "cp1@e.com", NID = "CP1", MobileNo = "CP1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "cpuser1", "OldPassword123");
            var result = await _service.ChangePasswordAsync(user.Id, "OldPassword123", "NewPassword456");

            result.Should().BeTrue();
            var updatedUser = await _context.Users.FindAsync(user.Id);
            BCrypt.Net.BCrypt.Verify("NewPassword456", updatedUser!.PasswordHash).Should().BeTrue();
        }

        [Test]
        public async Task ChangePasswordAsync_WithWrongOldPassword_ShouldReturnFalse()
        {
            var member = new Member { FullName = "Test Member", Email = "cp2@e.com", NID = "CP2", MobileNo = "CP2", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "cpuser2", "OldPassword123");
            var result = await _service.ChangePasswordAsync(user.Id, "WrongOldPassword", "NewPassword456");

            result.Should().BeFalse();
        }

        [Test]
        public async Task SendAdminPasswordResetLinkAsync_ForSystemAdmin_ShouldReturnUrlAndRevokeTokens()
        {
            // System admin accounts carry no Member/email, so the link is built from Username
            // and handed back rather than emailed. See AuthService.ResetPasswordAsync's matching
            // MemberId == null fallback for the consuming side.
            var user = new User { Username = "sysadmin1", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var (success, resetUrl) = await _service.SendAdminPasswordResetLinkAsync(user.Id);

            success.Should().BeTrue();
            resetUrl.Should().Contain("token=").And.Contain(Uri.EscapeDataString(user.Username));
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);

            var updated = await _context.Users.FindAsync(user.Id);
            updated!.ResetToken.Should().NotBeNullOrEmpty();
            updated.ResetTokenExpiry.Should().NotBeNull();
        }

        [Test]
        public async Task SendAdminPasswordResetLinkAsync_ForUnknownUser_ShouldReturnFalse()
        {
            var (success, resetUrl) = await _service.SendAdminPasswordResetLinkAsync(99999);

            success.Should().BeFalse();
            resetUrl.Should().BeNull();
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // SetUserActiveAsync/DeleteSystemAdminAsync's protected-username guard reads
        // AppSettings:ProtectedSuperAdmins — these tests need a specific list, so they build their
        // own options instead of using the fixture's default _appSettings.
        private UserService ServiceWithProtectedUsernames(params string[] protectedUsernames)
        {
            var options = Options.Create(new AppSettingsOptions { ProtectedSuperAdmins = protectedUsernames });
            return new UserService(_context, _mockLogger.Object, _mockTokenService.Object, options);
        }

        [Category("FR-11")]
        [Category("NFR-S5")]
        [Test]
        public async Task SetUserActiveAsync_Disable_ShouldRevokeTokensAndRotateStamp()
        {
            var user = new User { Username = "toggle1", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true, SecurityStamp = "old" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin");

            var result = await service.SetUserActiveAsync(user.Id, false);

            result.Should().BeTrue();
            var updated = await _context.Users.FindAsync(user.Id);
            updated!.IsActive.Should().BeFalse();
            updated.SecurityStamp.Should().NotBe("old");
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SetUserActiveAsync_Enable_ShouldNotRevokeTokens()
        {
            var user = new User { Username = "toggle2", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = false };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin");

            var result = await service.SetUserActiveAsync(user.Id, true);

            result.Should().BeTrue();
            (await _context.Users.FindAsync(user.Id))!.IsActive.Should().BeTrue();
            _mockTokenService.Verify(x => x.RevokeAllRefreshTokensAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SetUserActiveAsync_ProtectedUsername_ShouldReturnFalseAndLeaveUnchanged()
        {
            var user = new User { Username = "shalin", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin", "superadmin");

            var result = await service.SetUserActiveAsync(user.Id, false);

            result.Should().BeFalse();
            (await _context.Users.FindAsync(user.Id))!.IsActive.Should().BeTrue();
        }

        [Test]
        public async Task SetUserActiveAsync_UnknownUser_ShouldReturnFalse()
        {
            var service = ServiceWithProtectedUsernames();
            var result = await service.SetUserActiveAsync(99999, false);
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteSystemAdminAsync_ProtectedUsername_ShouldReturnFalseAndNotDelete()
        {
            var user = new User { Username = "superadmin", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin", "superadmin");

            var result = await service.DeleteSystemAdminAsync(user.Id);

            result.Should().BeFalse();
            (await _context.Users.FindAsync(user.Id)).Should().NotBeNull();
        }

        [Test]
        public async Task DeleteSystemAdminAsync_NonProtectedSystemAdmin_ShouldDelete()
        {
            var user = new User { Username = "regularadmin", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin");

            var result = await service.DeleteSystemAdminAsync(user.Id);

            result.Should().BeTrue();
            (await _context.Users.FindAsync(user.Id)).Should().BeNull();
        }
    }
}
