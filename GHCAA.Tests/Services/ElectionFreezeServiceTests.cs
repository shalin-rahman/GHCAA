using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 FR-039 (37.13q).
[TestFixture]
public sealed class ElectionFreezeServiceTests : TestBase
{

    private async Task<Election> AddElectionAsync(ElectionPhase phase)
    {
        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Frozen Test", ECPeriodId = period.Id, Phase = phase, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        return election;
    }

    private async Task AddOpenPollingRequestAsync(int electionId, bool rejected = false, bool expired = false)
    {
        var requester = new User { Username = $"requester-{Guid.NewGuid():N}", PasswordHash = "x" };
        _context.Users.Add(requester);
        await _context.SaveChangesAsync();
        _context.ElectionApprovals.Add(new ElectionApproval
        {
            ElectionId = electionId,
            Action = ElectionApprovalAction.OpenPolling,
            RequestedByUserId = requester.Id,
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddHours(1),
            RejectedAt = rejected ? DateTime.UtcNow : null,
        });
        await _context.SaveChangesAsync();
    }

    private Task<int> RefusalRowsAsync() =>
        _context.ActivityLogs.CountAsync(x => x.ActivityType == Constants.Elections.FrozenChangeRefusedAuditType);

    [TestCase(ElectionPhase.Polling)]
    [TestCase(ElectionPhase.Counting)]
    [Category("FR-39")]
    public async Task EnsureNotFrozen_PollingOrCounting_ThrowsAndRecordsRefusal(ElectionPhase phase)
    {
        var election = await AddElectionAsync(phase);

        var act = () => NewFreeze().EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Persona, new { id = 7 });

        (await act.Should().ThrowAsync<ElectionRulesFrozenException>()).Which.ElectionId.Should().Be(election.Id);
        var row = await _context.ActivityLogs.SingleAsync(x => x.ActivityType == Constants.Elections.FrozenChangeRefusedAuditType);
        row.Metadata.Should().Contain(Constants.Elections.FrozenRules.Persona).And.Contain("\"id\":7");
    }

    [TestCase(ElectionPhase.Campaign)]
    [TestCase(ElectionPhase.Declared)]
    [TestCase(ElectionPhase.Archived)]
    [Category("FR-39")]
    public async Task EnsureNotFrozen_OutsideWindow_Passes(ElectionPhase phase)
    {
        await AddElectionAsync(phase);

        await NewFreeze().EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, null);

        (await RefusalRowsAsync()).Should().Be(0);
    }

    [Test]
    [Category("FR-39")]
    public async Task EnsureNotFrozen_OpenPollingRequestWaiting_Throws()
    {
        var election = await AddElectionAsync(ElectionPhase.Campaign);
        await AddOpenPollingRequestAsync(election.Id);

        var act = () => NewFreeze().EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, null);

        await act.Should().ThrowAsync<ElectionRulesFrozenException>();
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [Category("FR-39")]
    public async Task EnsureNotFrozen_RejectedOrExpiredRequest_Passes(bool rejected, bool expired)
    {
        var election = await AddElectionAsync(ElectionPhase.Campaign);
        await AddOpenPollingRequestAsync(election.Id, rejected, expired);

        await NewFreeze().EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, null);
    }

    [Test]
    [Category("FR-39")]
    public async Task EnsureNotFrozen_OtherElectionPolling_DoesNotBlockThisOne()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var open = await AddElectionAsync(ElectionPhase.Campaign);

        await NewFreeze().EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Appointment, null, open.Id);
    }

    [Test]
    [Category("FR-39")]
    public async Task PersonaCreate_WhileFrozen_IsRefusedAndNothingSaved()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var service = new ElectionPersonaService(_context, NewFreeze());
        var dto = new SaveElectionPersonaDto { Name = "Late Persona", GroupName = Constants.Elections.PersonaGroups.Officials, Description = "d", DeclarationText = "I declare.", Permissions = ElectionPermission.Approve };

        var act = () => service.CreateAsync(dto, CancellationToken.None);

        await act.Should().ThrowAsync<ElectionRulesFrozenException>();
        (await _context.ElectionPersonas.AnyAsync(x => x.Name == "Late Persona")).Should().BeFalse();
    }

    [Test]
    [Category("FR-39")]
    public async Task OrgConfigSave_ElectionChangeWhileFrozen_IsRefused_OtherSectionsStillSave()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var service = new OrgConfigService(_context, new MemoryCache(new MemoryCacheOptions()), freeze: NewFreeze());
        var config = await service.GetConfigAsync();

        var act = () => service.UpdateConfigAsync(
            config with { Elections = config.Elections with { ApprovalExpiryHours = config.Elections.ApprovalExpiryHours + 1 } }, "1");
        await act.Should().ThrowAsync<ElectionRulesFrozenException>();

        await service.UpdateConfigAsync(config with { Branding = config.Branding with { MembershipNumberPrefix = "ZZ" } }, "1");
        (await service.GetConfigAsync()).Branding.MembershipNumberPrefix.Should().Be("ZZ");
        (await RefusalRowsAsync()).Should().Be(1);
    }

    private const string GoodReason = "Persona was saved with the wrong permission before polling.";

    private async Task<int> AddSuperAdminAsync()
    {
        var user = new User { Username = $"super-{Guid.NewGuid():N}", PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }

    [Test]
    [Category("FR-39")]
    public async Task Unlock_LetsAFrozenChangeThrough_AndLogsItWithTheUnlock()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var freeze = NewFreeze();
        var (error, unlock) = await freeze.OpenUnlockAsync(await AddSuperAdminAsync(), GoodReason, null);
        error.Should().BeNull();
        (unlock!.ExpiresAt - unlock.OpenedAt).Should().Be(TimeSpan.FromMinutes(Constants.Elections.RulesUnlockDefaultMinutes));

        await freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Persona, new { id = 7 });

        (await RefusalRowsAsync()).Should().Be(0);
        (await _context.ActivityLogs.CountAsync(x => x.ActivityType == Constants.Elections.RulesUnlockOpenedAuditType)).Should().Be(1);
        var row = await _context.ActivityLogs.SingleAsync(x => x.ActivityType == Constants.Elections.FrozenChangeUnlockedAuditType);
        row.Metadata.Should().Contain($"\"unlockId\":{unlock.Id}").And.Contain(GoodReason);
    }

    [Test]
    [Category("FR-39")]
    public async Task Unlock_NeverOpensAPlainRevoke()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var freeze = NewFreeze();
        await freeze.OpenUnlockAsync(await AddSuperAdminAsync(), GoodReason, null);

        var act = () => freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.AppointmentRevoke, new { appointmentId = 1 });

        await act.Should().ThrowAsync<ElectionRulesFrozenException>();
        (await RefusalRowsAsync()).Should().Be(1);
    }

    [Test]
    [Category("FR-39")]
    public async Task Unlock_TwoOpenedAtOnce_OneCloseShutsBoth()
    {
        await AddElectionAsync(ElectionPhase.Polling);
        var admin = await AddSuperAdminAsync();
        var now = DateTime.UtcNow;
        for (var i = 0; i < 2; i++)
            _context.ElectionRulesUnlocks.Add(new ElectionRulesUnlock { OpenedByUserId = admin, Reason = GoodReason, OpenedAt = now, ExpiresAt = now.AddMinutes(5) });
        await _context.SaveChangesAsync();
        var freeze = NewFreeze();

        (await freeze.CloseUnlockAsync(admin)).Should().BeNull();

        (await freeze.GetOpenUnlockAsync()).Should().BeNull();
        (await _context.ActivityLogs.CountAsync(a => a.ActivityType == Constants.Elections.RulesUnlockClosedAuditType)).Should().Be(2);
    }

    [Test]
    [Category("FR-39")]
    public async Task Unlock_ClosedEarlyOrExpired_LocksAgain()
    {
        await AddElectionAsync(ElectionPhase.Counting);
        var freeze = NewFreeze();
        var admin = await AddSuperAdminAsync();
        await freeze.OpenUnlockAsync(admin, GoodReason, 5);
        (await freeze.OpenUnlockAsync(admin, GoodReason, 5)).Error.Should().Be(Constants.Elections.RulesUnlockErrors.AlreadyOpen);

        (await freeze.CloseUnlockAsync(admin)).Should().BeNull();
        (await freeze.CloseUnlockAsync(admin)).Should().Be(Constants.Elections.RulesUnlockErrors.NotOpen);
        (await freeze.GetOpenUnlockAsync()).Should().BeNull();
        var closedEarly = () => freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, null);
        await closedEarly.Should().ThrowAsync<ElectionRulesFrozenException>();

        var (_, reopened) = await freeze.OpenUnlockAsync(admin, GoodReason, 5);
        await _context.ElectionRulesUnlocks.Where(u => u.Id == reopened!.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.ExpiresAt, DateTime.UtcNow.AddSeconds(-1)));
        var expired = () => freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, null);
        await expired.Should().ThrowAsync<ElectionRulesFrozenException>();
        (await _context.ActivityLogs.CountAsync(x => x.ActivityType == Constants.Elections.RulesUnlockClosedAuditType)).Should().Be(1);
    }

    [TestCase("too short", null, Constants.Elections.RulesUnlockErrors.ReasonTooShort)]
    [TestCase(GoodReason, 0, Constants.Elections.RulesUnlockErrors.InvalidMinutes)]
    [TestCase(GoodReason, Constants.Elections.RulesUnlockMaxMinutes + 1, Constants.Elections.RulesUnlockErrors.InvalidMinutes)]
    [Category("FR-39")]
    public async Task Unlock_BadRequest_IsRefused(string reason, int? minutes, string expected)
    {
        await AddElectionAsync(ElectionPhase.Polling);

        (await NewFreeze().OpenUnlockAsync(await AddSuperAdminAsync(), reason, minutes)).Error.Should().Be(expected);
        (await _context.ElectionRulesUnlocks.CountAsync()).Should().Be(0);
    }

    [Test]
    [Category("FR-39")]
    public async Task Unlock_NothingFrozen_IsRefused()
    {
        await AddElectionAsync(ElectionPhase.Campaign);

        (await NewFreeze().OpenUnlockAsync(await AddSuperAdminAsync(), GoodReason, null)).Error.Should().Be(Constants.Elections.RulesUnlockErrors.NotFrozen);
    }
}
