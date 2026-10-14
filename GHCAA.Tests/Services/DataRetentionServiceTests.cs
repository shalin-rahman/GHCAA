using FluentAssertions;
using GHCAA.API.Services;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class DataRetentionServiceTests : TestBase
    {
        private static readonly DateTime Now = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

        private static DateTime DaysAgo(int days) => Now.AddDays(-days);

        private Task PurgeAsync() => new DataRetentionService(_context).PurgeAsync(Now);

        private async Task<User> AddUserAsync(bool archived = false)
        {
            var member = await CreateAndSaveTestMemberAsync(email: $"m{Guid.NewGuid():N}@example.com", nid: Random.Shared.NextInt64(1_000_000_000, 9_999_999_999).ToString());
            member.IsArchived = archived;
            var user = await CreateAndSaveTestUserAsync(member.Id, $"u{Guid.NewGuid():N}");
            user.IsArchived = archived;
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task<int> SaveAsync<T>(T row) where T : class
        {
            _context.Add(row);
            await _context.SaveChangesAsync();
            return (int)_context.Entry(row).Property("Id").CurrentValue!;
        }

        // Each check reads past the query filters, so a row an archived user owns still counts.
        private Task<bool> ExistsAsync<T>(int id) where T : class =>
            _context.Set<T>().IgnoreQueryFilters().AnyAsync(e => EF.Property<int>(e, "Id") == id);

        [Test]
        public async Task Otps_OlderThanTheirPeriod_AreDeleted()
        {
            var old = await SaveAsync(new Otp { Email = "a@x.com", Code = "1", CreatedAt = DaysAgo(Constants.Retention.OtpDays + 1) });
            var recent = await SaveAsync(new Otp { Email = "a@x.com", Code = "2", CreatedAt = DaysAgo(Constants.Retention.OtpDays - 1) });

            await PurgeAsync();

            (await ExistsAsync<Otp>(old)).Should().BeFalse();
            (await ExistsAsync<Otp>(recent)).Should().BeTrue();
        }

        [Test]
        public async Task Backlog_LargerThanOneBatch_IsDeletedInFull()
        {
            var count = Constants.Retention.DeleteBatchSize + 3;
            var created = DaysAgo(Constants.Retention.OtpDays + 1);
            _context.Otps.AddRange(Enumerable.Range(0, count).Select(i => new Otp { Email = "a@x.com", Code = i.ToString(), CreatedAt = created }));
            await _context.SaveChangesAsync();

            var deleted = await new DataRetentionService(_context).PurgeAsync(Now);

            deleted["Otps"].Should().Be(count);
            (await _context.Otps.CountAsync()).Should().Be(0);
        }

        [Test]
        public async Task RefreshTokens_ExpiredOrRevokedLongAgo_AreDeleted()
        {
            var user = await AddUserAsync();
            var days = Constants.Retention.RefreshTokenDays;
            var expiredLongAgo = await SaveAsync(new RefreshToken { UserId = user.Id, TokenHash = "a", CreatedAt = DaysAgo(days + 10), ExpiresAt = DaysAgo(days + 1) });
            var revokedLongAgo = await SaveAsync(new RefreshToken { UserId = user.Id, TokenHash = "b", CreatedAt = DaysAgo(days + 1), ExpiresAt = Now.AddDays(1), IsRevoked = true });
            var expiredRecently = await SaveAsync(new RefreshToken { UserId = user.Id, TokenHash = "c", CreatedAt = DaysAgo(days), ExpiresAt = DaysAgo(days - 1) });
            var revokedRecently = await SaveAsync(new RefreshToken { UserId = user.Id, TokenHash = "d", CreatedAt = DaysAgo(days - 1), ExpiresAt = Now.AddDays(1), IsRevoked = true });

            await PurgeAsync();

            (await ExistsAsync<RefreshToken>(expiredLongAgo)).Should().BeFalse();
            (await ExistsAsync<RefreshToken>(revokedLongAgo)).Should().BeFalse();
            (await ExistsAsync<RefreshToken>(expiredRecently)).Should().BeTrue();
            (await ExistsAsync<RefreshToken>(revokedRecently)).Should().BeTrue();
        }

        [Test]
        public async Task RowsOfArchivedUsers_AreStillTrimmed()
        {
            var user = await AddUserAsync(archived: true);
            var token = await SaveAsync(new RefreshToken { UserId = user.Id, TokenHash = "a", CreatedAt = DaysAgo(400), ExpiresAt = DaysAgo(390) });
            var note = await SaveAsync(new Notification { MemberId = user.MemberId!.Value, Title = "t", Message = "m", IsRead = true, CreatedAt = DaysAgo(Constants.Retention.ReadNotificationDays + 1) });

            await PurgeAsync();

            (await ExistsAsync<RefreshToken>(token)).Should().BeFalse();
            (await ExistsAsync<Notification>(note)).Should().BeFalse();
        }

        [Test]
        public async Task Notifications_OnlyOldReadOnesAreDeleted()
        {
            var user = await AddUserAsync();
            var memberId = user.MemberId!.Value;
            var old = DaysAgo(Constants.Retention.ReadNotificationDays + 1);
            var oldRead = await SaveAsync(new Notification { MemberId = memberId, Title = "t", Message = "m", IsRead = true, CreatedAt = old });
            var oldUnread = await SaveAsync(new Notification { MemberId = memberId, Title = "t", Message = "m", IsRead = false, CreatedAt = old });
            var recentRead = await SaveAsync(new Notification { MemberId = memberId, Title = "t", Message = "m", IsRead = true, CreatedAt = DaysAgo(1) });

            await PurgeAsync();

            (await ExistsAsync<Notification>(oldRead)).Should().BeFalse();
            (await ExistsAsync<Notification>(oldUnread)).Should().BeTrue();
            (await ExistsAsync<Notification>(recentRead)).Should().BeTrue();
        }

        [Test]
        public async Task EmailLogs_OlderThanTheirPeriod_AreDeleted()
        {
            var old = await SaveAsync(new EmailLog { RecipientEmail = "a@x.com", Subject = "s", Body = "b", SentDate = DaysAgo(Constants.Retention.EmailLogDays + 1) });
            var recent = await SaveAsync(new EmailLog { RecipientEmail = "a@x.com", Subject = "s", Body = "b", SentDate = DaysAgo(Constants.Retention.EmailLogDays - 1) });

            await PurgeAsync();

            (await ExistsAsync<EmailLog>(old)).Should().BeFalse();
            (await ExistsAsync<EmailLog>(recent)).Should().BeTrue();
        }

        [Test]
        [Category("NFR-R5")]
        public async Task ActivityLogs_AuditRowsSurvive_OrdinaryRowsAreTrimmed()
        {
            var old = DaysAgo(Constants.Retention.ActivityLogDays + 1);
            var ordinary = await SaveAsync(new ActivityLog { ActivityType = "ApiAction", Description = "d", Timestamp = old });
            var vote = await SaveAsync(new ActivityLog { ActivityType = Constants.Elections.VoteAuditType, Description = "d", Timestamp = old });
            var origins = await SaveAsync(new ActivityLog { ActivityType = Constants.Deploy.AllowedOriginsChangedAuditType, Description = "d", Timestamp = old });
            var recent = await SaveAsync(new ActivityLog { ActivityType = "ApiAction", Description = "d", Timestamp = DaysAgo(1) });

            await PurgeAsync();

            (await ExistsAsync<ActivityLog>(ordinary)).Should().BeFalse();
            (await ExistsAsync<ActivityLog>(vote)).Should().BeTrue();
            (await ExistsAsync<ActivityLog>(origins)).Should().BeTrue();
            (await ExistsAsync<ActivityLog>(recent)).Should().BeTrue();
        }

        // 37.12s, the ten-year rule. An election table added to the purge list, or an election audit
        // type named without the kept prefix, would let the sweep delete a record the rule keeps.
        [Test]
        [Category("NFR-R5")]
        public void NoElectionRecordIsInReachOfTheSweep()
        {
            DataRetentionService.PurgedTables.Should().NotContain(t => t.Contains("Election", StringComparison.OrdinalIgnoreCase));

            var auditTypes = typeof(Constants).GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
                .SelectMany(AllNested)
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
                .Where(f => f.IsLiteral && f.Name.EndsWith("AuditType") && f.DeclaringType!.FullName!.Contains("Elections"))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();

            auditTypes.Should().NotBeEmpty();
            auditTypes.Should().OnlyContain(t => t.StartsWith(Constants.Retention.KeptActivityTypePrefix));
        }

        private static IEnumerable<Type> AllNested(Type t) =>
            new[] { t }.Concat(t.GetNestedTypes(BindingFlags.Public).SelectMany(AllNested));

        [Test]
        public async Task Sweep_RunsThePurgeInItsOwnScope()
        {
            var retention = new Mock<IDataRetentionService>();
            retention.Setup(r => r.PurgeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, int> { ["Otps"] = 2 });
            var sweep = NewSweep(retention.Object);

            await sweep.RunOnceAsync(CancellationToken.None);

            retention.Verify(r => r.PurgeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Sweep_ThatFails_DoesNotThrow()
        {
            var retention = new Mock<IDataRetentionService>();
            retention.Setup(r => r.PurgeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db down"));
            var sweep = NewSweep(retention.Object);

            var act = () => sweep.RunOnceAsync(CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        private static DataRetentionSweep NewSweep(IDataRetentionService retention)
        {
            var provider = new ServiceCollection().AddScoped(_ => retention).BuildServiceProvider();
            return new DataRetentionSweep(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<DataRetentionSweep>.Instance);
        }
    }
}
