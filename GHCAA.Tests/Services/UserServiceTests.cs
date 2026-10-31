using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class UserServiceTests : TestBase
    {
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private Mock<ITokenService> _mockTokenService = null!;
        private IOptions<AppSettingsOptions> _appSettings = null!;
        private Mock<IActivityService> _mockActivity = null!;
        private UserService _service = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockTokenService = new Mock<ITokenService>();
            _appSettings = Options.Create(new AppSettingsOptions());
            _mockActivity = new Mock<IActivityService>();
            _service = new UserService(_context, _mockLogger.Object, _mockTokenService.Object, _appSettings, _mockActivity.Object);
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

        [Category("FR-39")]
        [Test]
        public async Task RecordFailedPasswordAsync_LocksOnFifthFailureAndCanReset()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var user = await CreateAndSaveTestUserAsync(member.Id, "atomic-lockout");

            for (var i = 0; i < Constants.Login.MaxFailedAttempts; i++)
                (await _service.RecordFailedPasswordAsync(user.Id)).Should().BeTrue();

            var locked = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            locked.FailedLoginAttempts.Should().Be(Constants.Login.MaxFailedAttempts);
            locked.LockoutUntil.Should().BeAfter(DateTime.UtcNow);

            (await _service.ResetFailedPasswordsAsync(user.Id)).Should().BeTrue();
            var reset = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            reset.FailedLoginAttempts.Should().Be(0);
            reset.LockoutUntil.Should().BeNull();
        }

        [Category("FR-39")]
        [Test]
        public async Task UnlockUserAsync_ClearsLoginAndAuthenticatorLocksAndLogsIt()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var user = await CreateAndSaveTestUserAsync(member.Id, "locked-out");
            var lockedUntil = DateTime.UtcNow.AddMinutes(10);
            user.FailedLoginAttempts = Constants.Login.MaxFailedAttempts;
            user.LockoutUntil = lockedUntil;
            _context.UserAuthenticators.Add(new UserAuthenticator
            {
                UserId = user.Id, ProtectedSecret = "secret", ConfirmedAt = DateTime.UtcNow,
                FailedAttempts = Constants.Authenticator.MaxFailedAttempts, LockedUntil = lockedUntil,
            });
            await _context.SaveChangesAsync();

            (await _service.UnlockUserAsync(user.Id, adminId: 99)).Should().BeTrue();

            var unlocked = await _context.Users.AsNoTracking().SingleAsync(u => u.Id == user.Id);
            unlocked.FailedLoginAttempts.Should().Be(0);
            unlocked.LockoutUntil.Should().BeNull();
            var app = await _context.UserAuthenticators.AsNoTracking().SingleAsync(a => a.UserId == user.Id);
            app.FailedAttempts.Should().Be(0);
            app.LockedUntil.Should().BeNull();
            _mockActivity.Verify(a => a.LogActivityAsync(member.Id, Constants.Authenticator.AdminUnlockAuditType,
                It.IsAny<string>(), 99, null, null, null, null, It.IsAny<CancellationToken>(), null), Times.Once);
        }

        [Category("FR-39")]
        [Test]
        public async Task UnlockUserAsync_UnknownUser_ReturnsFalseAndLogsNothing()
        {
            (await _service.UnlockUserAsync(424242, adminId: 99)).Should().BeFalse();

            _mockActivity.Verify(a => a.LogActivityAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()), Times.Never);
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
            return new UserService(_context, _mockLogger.Object, _mockTokenService.Object, options, _mockActivity.Object);
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
            var user = new User { Username = "shalin_protected_test", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin_protected_test", "superadmin_protected_test");

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
            var user = new User { Username = "superadmin_protected_test", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin_protected_test", "superadmin_protected_test");

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

        // 7.23. Opening a rules unlock leaves a Restrict key on the user, so the delete is refused and the row stays.
        [Test]
        public async Task DeleteSystemAdminAsync_UserWithElectionRecords_IsRefusedAndKept()
        {
            var user = new User { Username = "unlockadmin", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _context.ElectionRulesUnlocks.Add(new ElectionRulesUnlock { OpenedByUserId = user.Id, Reason = "r", OpenedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddMinutes(30) });
            await _context.SaveChangesAsync();
            var service = ServiceWithProtectedUsernames("shalin");

            (await service.HasElectionRecordsAsync(user.Id)).Should().BeTrue();
            (await service.DeleteSystemAdminAsync(user.Id)).Should().BeFalse();
            (await _context.Users.AsNoTracking().AnyAsync(x => x.Id == user.Id)).Should().BeTrue();
        }

        [Test]
        public async Task HasElectionRecordsAsync_UserWithNone_IsFalse()
        {
            var user = new User { Username = "plainadmin", PasswordHash = "x", MemberId = null, CreatedAt = DateTime.UtcNow, IsActive = true };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            (await ServiceWithProtectedUsernames("shalin").HasElectionRecordsAsync(user.Id)).Should().BeFalse();
        }
    }
}
