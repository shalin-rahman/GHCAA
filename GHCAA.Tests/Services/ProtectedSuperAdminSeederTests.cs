using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
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
    }
}
