using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace GHCAA.Tests.Data
{
    // 98.8. The middleware caches each user's stamp for 30 seconds. These pin the eviction that
    // keeps a rotation effective on the next request, and the rule that makes eviction reliable.
    [TestFixture]
    public class SecurityStampCacheInterceptorTests
    {
        private SqliteConnection _connection = null!;
        private MemoryCache _cache = null!;
        private ApplicationDbContext _context = null!;

        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _cache = new MemoryCache(new MemoryCacheOptions());
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SecurityStampCacheInterceptor(_cache))
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _cache.Dispose();
            _connection.Close();
        }

        private async Task<User> AddUserAsync()
        {
            var user = new User { Username = "stamp-user", PasswordHash = "ph" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _cache.Set(Constants.SecurityStamps.CacheKey(user.Id), user.SecurityStamp);
            return user;
        }

        [Test]
        [Category("NFR-S5")]
        public async Task RotatedStamp_DropsTheCachedCopyOnSave()
        {
            var user = await AddUserAsync();

            user.RotateSecurityStamp();
            await _context.SaveChangesAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out _).Should().BeFalse();
        }

        // The row is gone before the save, so the update fails and the stored stamp never changes.
        [Test]
        [Category("NFR-S5")]
        public async Task FailedSave_KeepsTheCachedCopy()
        {
            var user = await AddUserAsync();
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE Id = {0}", user.Id);

            user.RotateSecurityStamp();
            var act = () => _context.SaveChangesAsync();

            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out _).Should().BeTrue();
        }

        [Test]
        [Category("NFR-S5")]
        public void RotatedStamp_SyncSave_DropsTheCachedCopy()
        {
            var user = AddUserAsync().GetAwaiter().GetResult();

            user.RotateSecurityStamp();
            _context.SaveChanges();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out _).Should().BeFalse();
        }

        [Test]
        public async Task SaveThatLeavesTheStampAlone_KeepsTheCachedCopy()
        {
            var user = await AddUserAsync();

            user.FailedLoginAttempts = 2;
            await _context.SaveChangesAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out string? cached).Should().BeTrue();
            cached.Should().Be(user.SecurityStamp);
        }

        [Test]
        public async Task RotationOfOneUser_LeavesOtherUsersCached()
        {
            var first = await AddUserAsync();
            var second = new User { Username = "other-user", PasswordHash = "ph" };
            _context.Users.Add(second);
            await _context.SaveChangesAsync();
            _cache.Set(Constants.SecurityStamps.CacheKey(second.Id), second.SecurityStamp);

            first.RotateSecurityStamp();
            await _context.SaveChangesAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(second.Id), out _).Should().BeTrue();
        }

        // The middleware can read the old stamp from the database and cache it again between the
        // save and the commit. The commit has to drop that copy too.
        [Test]
        [Category("NFR-S5")]
        public async Task RotationInATransaction_StampCachedBeforeCommit_IsDroppedAtCommit()
        {
            var user = await AddUserAsync();
            var oldStamp = user.SecurityStamp;
            await using var tx = await _context.Database.BeginTransactionAsync();

            user.RotateSecurityStamp();
            await _context.SaveChangesAsync();
            _cache.Set(Constants.SecurityStamps.CacheKey(user.Id), oldStamp);
            await tx.CommitAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out _).Should().BeFalse();
        }

        [Test]
        [Category("NFR-S5")]
        public async Task RolledBackRotation_IsNotEvictedByALaterCommit()
        {
            var user = await AddUserAsync();
            await using (var tx = await _context.Database.BeginTransactionAsync())
            {
                user.RotateSecurityStamp();
                await _context.SaveChangesAsync();
                await tx.RollbackAsync();
            }
            _context.ChangeTracker.Clear();
            var stamp = (await _context.Users.SingleAsync(u => u.Id == user.Id)).SecurityStamp;
            _cache.Set(Constants.SecurityStamps.CacheKey(user.Id), stamp);

            await using (var next = await _context.Database.BeginTransactionAsync())
                await next.CommitAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out string? cached).Should().BeTrue();
            cached.Should().Be(stamp);
        }

        // Disposing a transaction without commit or rollback raises neither event. Its ids must not
        // wait for the next commit on the context.
        [Test]
        [Category("NFR-S5")]
        public async Task AbandonedTransaction_IsNotEvictedByALaterCommit()
        {
            var user = await AddUserAsync();
            await using (var tx = await _context.Database.BeginTransactionAsync())
            {
                user.RotateSecurityStamp();
                await _context.SaveChangesAsync();
            }
            _cache.Set(Constants.SecurityStamps.CacheKey(user.Id), "fresh");

            await using (var next = await _context.Database.BeginTransactionAsync())
                await next.CommitAsync();

            _cache.TryGetValue(Constants.SecurityStamps.CacheKey(user.Id), out string? cached).Should().BeTrue();
            cached.Should().Be("fresh");
        }

        // A stamp written any other way (a direct assignment, or ExecuteUpdate, which skips the
        // change tracker) would leave a stale stamp cached and an ended session alive for 30 seconds.
        [Test]
        [Category("NFR-S5")]
        public void StampIsOnlyChangedThroughRotateSecurityStamp()
        {
            var assignment = new Regex(@"\.SecurityStamp\s*=(?!=)");
            var bulkUpdate = new Regex(@"SetProperty\(\s*\w+\s*=>\s*\w+\.SecurityStamp\b");
            var offenders = new List<string>();

            foreach (var project in new[] { "GHCAA.API", "GHCAA.Application", "GHCAA.Infrastructure" })
            {
                var files = Directory.EnumerateFiles(Path.Combine(RepoRoot(), project), "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"));
                foreach (var file in files)
                {
                    var lines = File.ReadAllLines(file);
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (assignment.IsMatch(lines[i]) || bulkUpdate.IsMatch(lines[i]))
                            offenders.Add($"{Path.GetRelativePath(RepoRoot(), file)}:{i + 1}");
                    }
                }
            }

            offenders.Should().BeEmpty("a stamp change must go through User.RotateSecurityStamp() and a tracked save");
        }

        private static string RepoRoot()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            for (var i = 0; i < 6; i++)
            {
                if (Directory.Exists(Path.Combine(dir, "GHCAA.Infrastructure")) && Directory.Exists(Path.Combine(dir, "GHCAA.API")))
                    return dir;
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }
            throw new DirectoryNotFoundException("Could not locate the repo root from the test output directory.");
        }
    }
}
