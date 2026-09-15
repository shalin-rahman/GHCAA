using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    // Regression coverage for TODO 44.18: a protected account's SuperAdmin role must survive
    // a full Users-table wipe+reseed (the shape of Program.cs's Visual-profile
    // OverrideEFCoreMigratedData path), as long as ProtectedSuperAdminSeeder.EnsureAsync runs
    // after that reseed rather than before it.
    [TestFixture]
    public class ProtectedSuperAdminSeederTests : TestBase
    {
        private Mock<ILogger> _mockLogger = null!;

        private IUserService _userService = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _userService = new UserService(
                _context,
                new Mock<ILogger<UserService>>().Object,
                new Mock<ITokenService>().Object,
                Options.Create(new AppSettingsOptions()));
        }

        [Test]
        public async Task EnsureAsync_GrantsSuperAdminRole_ToProtectedUsernameWithNoRoles()
        {
            var member = await CreateAndSaveTestMemberAsync("Protected Test Admin", "protected-test-admin@e.com", "111", "111");
            await CreateAndSaveTestUserAsync(member.Id, "protected-test-admin");

            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "protected-test-admin" }, _mockLogger.Object);

            var user = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            user.Roles.Should().Contain(r => r.Name == "SuperAdmin");
        }

        [Test]
        public async Task EnsureAsync_IsIdempotent_WhenRoleAlreadyGranted()
        {
            var member = await CreateAndSaveTestMemberAsync("Protected Test Admin", "protected-test-admin@e.com", "111", "111");
            await CreateAndSaveTestUserAsync(member.Id, "protected-test-admin");

            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "protected-test-admin" }, _mockLogger.Object);
            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "protected-test-admin" }, _mockLogger.Object);

            var user = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            user.Roles.Count(r => r.Name == "SuperAdmin").Should().Be(1);
        }

        [Test]
        public async Task EnsureAsync_RestoresRole_AfterSimulatedVisualProfileUserWipeAndReseed()
        {
            var member = await CreateAndSaveTestMemberAsync("Protected Test Admin", "protected-test-admin@e.com", "111", "111");
            await CreateAndSaveTestUserAsync(member.Id, "protected-test-admin");
            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "protected-test-admin" }, _mockLogger.Object);

            var restored = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            restored.Roles.Should().Contain(r => r.Name == "SuperAdmin");

            // Simulate Program.cs's OverrideEFCoreMigratedData: wipe every User row and
            // re-insert from a source with no role data (mirrors Seed/Visual/users.json).
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();
            _context.Users.Add(new User
            {
                Username = "protected-test-admin",
                PasswordHash = restored.PasswordHash,
                MemberId = member.Id,
                CreatedAt = restored.CreatedAt,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var wiped = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            wiped.Roles.Should().BeEmpty("the reseed carries no role data, matching the pre-fix bug");

            // Re-running the seeder AFTER the reseed (the fixed Program.cs ordering) must restore it.
            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "protected-test-admin" }, _mockLogger.Object);

            var healed = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            healed.Roles.Should().Contain(r => r.Name == "SuperAdmin");
        }

        [Test]
        public async Task EnsureAsync_SkipsUnknownUsername_WithoutThrowing()
        {
            var act = async () => await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "does-not-exist" }, _mockLogger.Object);
            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task EnsureAsync_NoOps_WhenProtectedListIsEmpty()
        {
            var member = await CreateAndSaveTestMemberAsync("Protected Test Admin", "protected-test-admin@e.com", "111", "111");
            await CreateAndSaveTestUserAsync(member.Id, "protected-test-admin");

            await ProtectedSuperAdminSeeder.EnsureAsync(_context, System.Array.Empty<string>(), _mockLogger.Object);

            var user = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            user.Roles.Should().BeEmpty();
        }

        // Coverage for TODO 62.50: on a fresh database with no SuperAdmin at all, EnsureAsync
        // above has nothing to re-grant a role to, so nobody could ever log in as admin.
        // BootstrapFirstSuperAdminAsync creates that first account instead.
        [Test]
        public async Task BootstrapFirstSuperAdminAsync_CreatesSuperAdmin_WhenDatabaseHasNone()
        {
            // The seeded Users table already has a "shalin" row (TestBase runs the real
            // users.json seed), so clear it to actually exercise the "database has none" case.
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();

            await ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync(
                _context, new[] { "shalin" }, _userService, _mockLogger.Object);

            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == "shalin");
            user.Should().NotBeNull();
            user!.Roles.Should().Contain(r => r.Name == "SuperAdmin");
            user.MustChangePassword.Should().BeTrue("the password is machine-generated and must be rotated");
        }

        [Test]
        public async Task BootstrapFirstSuperAdminAsync_NoOps_WhenASuperAdminAlreadyExists()
        {
            // The seeded Users table already has a "shalin" row; clear it so the only
            // superadmin present is the one this test creates below.
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();

            var member = await CreateAndSaveTestMemberAsync("Existing Admin", "existing-admin@e.com", "222", "222");
            await CreateAndSaveTestUserAsync(member.Id, "existing-admin");
            await ProtectedSuperAdminSeeder.EnsureAsync(_context, new[] { "existing-admin" }, _mockLogger.Object);

            await ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync(
                _context, new[] { "shalin" }, _userService, _mockLogger.Object);

            (await _context.Users.AnyAsync(u => u.Username == "shalin")).Should().BeFalse();
        }

        [Test]
        public async Task BootstrapFirstSuperAdminAsync_NoOps_WhenProtectedUsernameExistsWithoutTheRole()
        {
            var member = await CreateAndSaveTestMemberAsync("Protected Test Admin", "protected-test-admin@e.com", "111", "111");
            await CreateAndSaveTestUserAsync(member.Id, "protected-test-admin");

            await ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync(
                _context, new[] { "protected-test-admin" }, _userService, _mockLogger.Object);

            var user = await _context.Users.Include(u => u.Roles).FirstAsync(u => u.Username == "protected-test-admin");
            user.Roles.Should().BeEmpty("this account belongs to EnsureAsync, not the bootstrap");
            (await _context.Users.CountAsync(u => u.Username == "protected-test-admin")).Should().Be(1);
        }

        [Test]
        public async Task BootstrapFirstSuperAdminAsync_WritesGeneratedPasswordToFile()
        {
            var passwordFilePath = Path.Combine(Path.GetTempPath(), $"superadmin-bootstrap-{Guid.NewGuid():N}.txt");
            try
            {
                // The seeded Users table already has a "shalin" row, which would make the
                // bootstrap no-op and never write the password file.
                _context.Users.RemoveRange(_context.Users);
                await _context.SaveChangesAsync();

                await ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync(
                    _context, new[] { "shalin" }, _userService, _mockLogger.Object, passwordFilePath);

                File.Exists(passwordFilePath).Should().BeTrue();
                var contents = await File.ReadAllTextAsync(passwordFilePath);
                contents.Should().Contain("Username: shalin").And.Contain("Password: ");
            }
            finally
            {
                if (File.Exists(passwordFilePath)) File.Delete(passwordFilePath);
            }
        }
    }
}
