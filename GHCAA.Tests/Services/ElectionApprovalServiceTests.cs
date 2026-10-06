using System.Security.Cryptography;
using FluentAssertions;
using GHCAA.API.Services;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 (37.12f): the two-person rule. Real election and access services run against the test
// database, so a step that runs really changes the election.
[TestFixture]
[Category("FR-39")]
public sealed class ElectionApprovalServiceTests : TestBase
{
    private static readonly string[] Admin = [Constants.Roles.Admin];
    private static readonly string[] SuperAdmin = [Constants.Roles.SuperAdmin];
    private static readonly string[] Official = [Constants.Roles.ElectionOfficial];

    private Mock<IOrgConfigService> _orgConfig = null!;
    private ElectionApprovalService _service = null!;
    private Election _election = null!;
    private int _alice;
    private int _bob;
    private int _carol;

    [SetUp]
    public async Task SetUpFixture()
    {
        _orgConfig = new Mock<IOrgConfigService>();
        UseSettings(new ElectionSettingsDto());
        _service = new ElectionApprovalService(_context, new ElectionService(_context), new ElectionAccessService(_context, _orgConfig.Object), _orgConfig.Object, NewAppointments(_orgConfig.Object), NewActivity(), NullLogger<ElectionApprovalService>.Instance, NewFreeze());

        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _election = new Election { Title = "Election", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(_election);
        var users = new[] { "alice", "bob", "carol" }.Select(n => new User { Username = n, PasswordHash = "x" }).ToArray();
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
        (_alice, _bob, _carol) = (users[0].Id, users[1].Id, users[2].Id);
    }

    private void UseSettings(ElectionSettingsDto settings) =>
        _orgConfig.Setup(s => s.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Elections = settings });

    private async Task<Election> ReloadAsync() => await _context.Elections.AsNoTracking().SingleAsync(x => x.Id == _election.Id);

    private static string NewPublicKey()
    {
        using var rsa = RSA.Create(Constants.Elections.BallotKeyMinBits);
        return Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
    }

    private async Task<ElectionApprovalDto> RequestPublishAsync()
    {
        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, Admin);
        run.Pending.Should().NotBeNull();
        return run.Pending!;
    }

    [Test]
    public async Task TwoPersonStep_IsStored_AndDoesNotRun()
    {
        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, Admin);

        run.Ran.Should().BeFalse();
        run.Error.Should().BeNull();
        run.Pending!.RequestedBy.Should().Be("alice");
        run.Pending.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromMinutes(1));
        (await ReloadAsync()).Phase.Should().Be(ElectionPhase.Announced);
        (await _service.ListOpenAsync(_election.Id)).Should().ContainSingle(x => x.Id == run.Pending.Id);
    }

    [Test]
    public async Task SecondRequest_ForTheSameStep_IsRefused()
    {
        await RequestPublishAsync();

        var again = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _bob, Admin);

        again.Error.Should().Be("already-pending");
        again.Pending.Should().BeNull();
    }

    [Test]
    public async Task Requester_CannotApproveTheirOwnRequest()
    {
        var pending = await RequestPublishAsync();

        (await _service.ApproveAsync(pending.Id, _alice, Admin)).Error.Should().Be("same-person");
        (await ReloadAsync()).Phase.Should().Be(ElectionPhase.Announced);
    }

    [Test]
    public async Task ApproverWithoutApprove_IsRefused()
    {
        var pending = await RequestPublishAsync();

        (await _service.ApproveAsync(pending.Id, _carol, Official)).Error.Should().Be("forbidden");
        (await _service.RejectAsync(pending.Id, _carol, Official, null)).Error.Should().Be("forbidden");
    }

    [Test]
    public async Task SecondPerson_RunsTheStepOnce()
    {
        var pending = await RequestPublishAsync();

        (await _service.ApproveAsync(pending.Id, _bob, Admin)).Success.Should().BeTrue();
        (await ReloadAsync()).Phase.Should().Be(ElectionPhase.Nomination);

        (await _service.ApproveAsync(pending.Id, _carol, SuperAdmin)).Error.Should().Be("closed");
        (await _service.ListOpenAsync(_election.Id)).Should().BeEmpty();
        var row = await _context.ElectionApprovals.AsNoTracking().SingleAsync(x => x.Id == pending.Id);
        row.ApprovedByUserId.Should().Be(_bob);
        row.ExecutedAt.Should().NotBeNull();
        row.RejectedAt.Should().BeNull();
    }

    [Test]
    public async Task StepThatRan_CannotBeRejectedAfter()
    {
        var pending = await RequestPublishAsync();
        (await _service.ApproveAsync(pending.Id, _bob, Admin)).Success.Should().BeTrue();

        (await _service.RejectAsync(pending.Id, _carol, SuperAdmin, "too late")).Error.Should().Be("closed");
        var row = await _context.ElectionApprovals.AsNoTracking().SingleAsync(x => x.Id == pending.Id);
        row.RejectedAt.Should().BeNull();
        row.RejectReason.Should().BeNull();
    }

    [Test]
    public async Task FailedStep_LeavesTheRequestOpen()
    {
        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ClosePolling, _alice, Admin);

        // Announced cannot jump to Counting, so the step fails and the claim rolls back.
        (await _service.ApproveAsync(run.Pending!.Id, _bob, Admin)).Error.Should().Be("not-ready");
        (await _service.ListOpenAsync(_election.Id)).Should().ContainSingle();
    }

    [Test]
    public async Task ExpiredRequest_IsRefused()
    {
        var pending = await RequestPublishAsync();
        await _context.ElectionApprovals.Where(x => x.Id == pending.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));

        (await _service.ApproveAsync(pending.Id, _bob, Admin)).Error.Should().Be("expired");
        (await _service.ListOpenAsync(_election.Id)).Should().BeEmpty();
        // An expired request no longer blocks a fresh one.
        (await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, Admin)).Pending.Should().NotBeNull();
    }

    [Test]
    public async Task RejectedRequest_NeverRuns()
    {
        var pending = await RequestPublishAsync();

        (await _service.RejectAsync(pending.Id, _bob, Admin, "  wrong dates  ")).Success.Should().BeTrue();

        (await _service.ApproveAsync(pending.Id, _carol, SuperAdmin)).Error.Should().Be("closed");
        (await ReloadAsync()).Phase.Should().Be(ElectionPhase.Announced);
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync(x => x.Id == pending.Id)).RejectReason.Should().Be("wrong dates");
    }

    [Test]
    public async Task StepNotInTheList_RunsAtOnce()
    {
        UseSettings(new ElectionSettingsDto { TwoPersonActions = [nameof(ElectionApprovalAction.Declare)] });

        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, Admin);

        run.Ran.Should().BeTrue();
        run.Error.Should().BeNull();
        (await ReloadAsync()).Phase.Should().Be(ElectionPhase.Nomination);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task SuperAdmin_ActsAlone_OnlyWhenTheSwitchIsOn(bool actsAlone, bool expectRan)
    {
        UseSettings(new ElectionSettingsDto { SuperAdminActsAlone = actsAlone });

        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, SuperAdmin);

        run.Ran.Should().Be(expectRan);
        (await ReloadAsync()).Phase.Should().Be(expectRan ? ElectionPhase.Nomination : ElectionPhase.Announced);
    }

    [Test]
    public async Task SwitchOn_DoesNotLetAnAdminActAlone()
    {
        UseSettings(new ElectionSettingsDto { SuperAdminActsAlone = true });

        (await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.Publish, _alice, Admin)).Ran.Should().BeFalse();
    }

    [Test]
    public async Task FirstKey_IsSetAtOnce_AndAReplacementWaits_ThenStoresTheApprovedKey()
    {
        var first = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, NewPublicKey());
        first.Ran.Should().BeTrue();
        first.Error.Should().BeNull();
        var firstFingerprint = (await ReloadAsync()).BallotKeyFingerprint;
        firstFingerprint.Should().NotBeNull();

        var replace = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, NewPublicKey());
        replace.Ran.Should().BeFalse();
        replace.Pending!.KeyFingerprint.Should().NotBeNull().And.NotBe(firstFingerprint);
        (await ReloadAsync()).BallotKeyFingerprint.Should().Be(firstFingerprint);

        (await _service.ApproveAsync(replace.Pending.Id, _bob, Admin)).Success.Should().BeTrue();
        (await ReloadAsync()).BallotKeyFingerprint.Should().Be(replace.Pending.KeyFingerprint);
    }

    [Test]
    [Category("FR-39")]
    public async Task KeyReplacement_WhileOpenPollingWaits_IsLocked()
    {
        await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, NewPublicKey());
        var replace = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, NewPublicKey());
        var fingerprint = (await ReloadAsync()).BallotKeyFingerprint;
        (await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.OpenPolling, _alice, Admin)).Pending.Should().NotBeNull();

        var approve = () => _service.ApproveAsync(replace.Pending!.Id, _bob, Admin);
        var again = () => _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _carol, Admin, NewPublicKey());

        await approve.Should().ThrowAsync<ElectionRulesFrozenException>();
        await again.Should().ThrowAsync<ElectionRulesFrozenException>();
        (await ReloadAsync()).BallotKeyFingerprint.Should().Be(fingerprint);
        (await _context.ActivityLogs.CountAsync(x => x.ActivityType == Constants.Elections.FrozenChangeRefusedAuditType)).Should().Be(2);
    }

    [Test]
    public async Task KeyReplacement_WithABadKey_IsRefusedBeforeStoring()
    {
        await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, NewPublicKey());

        var run = await _service.RunOrRequestAsync(_election.Id, ElectionApprovalAction.ReplaceBallotKey, _alice, Admin, "not a key");

        run.Error.Should().Be("invalid-key");
        (await _service.ListOpenAsync(_election.Id)).Should().BeEmpty();
    }

    [Test]
    public async Task UnknownElectionOrApproval_IsNotFound()
    {
        (await _service.RunOrRequestAsync(999_999, ElectionApprovalAction.Publish, _alice, Admin)).Error.Should().Be("not-found");
        (await _service.ApproveAsync(999_999, _bob, Admin)).Error.Should().Be("not-found");
        (await _service.RejectAsync(999_999, _bob, Admin, null)).Error.Should().Be("not-found");
    }

    // 37.13h. The count is mocked here: these tests are about who may count and how often. The
    // count itself is covered in ElectionServiceTests.
    private const string Key = "pkcs8";
    private static readonly IReadOnlyList<ElectionResultDto> Counted = [new ElectionResultDto(1, 1, 10, true, false)];

    private async Task<(ElectionApprovalService Service, Mock<IElectionService> Elections)> CountingAsync()
    {
        await _context.Elections.Where(x => x.Id == _election.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Phase, ElectionPhase.Counting));
        var elections = new Mock<IElectionService>();
        elections.Setup(x => x.CountAsync(_election.Id, null, It.IsAny<CancellationToken>())).ReturnsAsync(((IReadOnlyList<ElectionResultDto>?)null, "no-key"));
        elections.Setup(x => x.CountAsync(_election.Id, Key, It.IsAny<CancellationToken>())).ReturnsAsync(((IReadOnlyList<ElectionResultDto>?)Counted, (string?)null));
        var service = new ElectionApprovalService(_context, elections.Object, new ElectionAccessService(_context, _orgConfig.Object), _orgConfig.Object, NewAppointments(_orgConfig.Object), NewActivity(), NullLogger<ElectionApprovalService>.Instance, NewFreeze());
        return (service, elections);
    }

    [Test]
    public async Task Count_FirstCallWaitsForASecondPerson_AndDoesNotOpenBallots()
    {
        var (service, elections) = await CountingAsync();

        var run = await service.CountAsync(_election.Id, _alice, Admin, Key);

        run.Results.Should().BeNull();
        run.Pending!.Action.Should().Be(ElectionApprovalAction.Count);
        run.Pending.ApprovedAt.Should().BeNull();
        elections.Verify(x => x.CountAsync(_election.Id, Key, It.IsAny<CancellationToken>()), Times.Never);
        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Error.Should().Be("already-pending");
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync()).PayloadJson.Should().BeNull();
    }

    [Test]
    public async Task Count_WithNoKey_DoesNotStoreARequest()
    {
        var (service, _) = await CountingAsync();

        (await service.CountAsync(_election.Id, _alice, Admin, null)).Error.Should().Be("no-key");

        _context.ElectionApprovals.Should().BeEmpty();
    }

    [Test]
    public async Task Count_AfterApproval_RunsOnceForAnyoneButTheApprover()
    {
        var (service, elections) = await CountingAsync();
        var pending = (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending!;
        (await service.ApproveAsync(pending.Id, _bob, Admin)).Success.Should().BeTrue();
        elections.Verify(x => x.CountAsync(_election.Id, Key, It.IsAny<CancellationToken>()), Times.Never);
        (await service.ListOpenAsync(_election.Id)).Should().ContainSingle(x => x.Id == pending.Id && x.ApprovedAt != null);

        (await service.RequestAsync(_election.Id, ElectionApprovalAction.Count, _carol, null)).Error.Should().Be("already-pending");
        (await service.CountAsync(_election.Id, _bob, Admin, Key)).Error.Should().Be("same-person");
        (await service.CountAsync(_election.Id, _alice, Admin, null)).Error.Should().Be("no-key");
        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Results.Should().BeEquivalentTo(Counted);

        elections.Verify(x => x.CountAsync(_election.Id, Key, It.IsAny<CancellationToken>()), Times.Once);
        var used = await _context.ElectionApprovals.AsNoTracking().SingleAsync();
        used.ConsumedAt.Should().NotBeNull();
        // 37.13m. Requester, approver and executor are kept apart.
        (used.RequestedByUserId, used.ApprovedByUserId, used.ExecutedByUserId).Should().Be((_alice, (int?)_bob, (int?)_alice));
        var audit = await _context.ActivityLogs.AsNoTracking().SingleAsync(x => x.ActivityType == Constants.Elections.CountRunAuditType);
        audit.ActorId.Should().Be(_alice);
        audit.Metadata.Should().Contain($"\"approverId\":{_bob}").And.Contain($"\"executorId\":{_alice}");
        (await service.ListOpenAsync(_election.Id)).Should().BeEmpty();
        // The approval is used up, so another count needs another one.
        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending.Should().NotBeNull();
    }

    // 37.13n, spec 023 FR-036.
    [Test]
    public async Task Count_RequesterOnly_RefusesAnyoneElse_AndKeepsTheApproval()
    {
        var (service, elections) = await CountingAsync();
        UseSettings(new ElectionSettingsDto { CountRequesterOnly = true });
        var pending = (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending!;
        await service.ApproveAsync(pending.Id, _bob, Admin);

        (await service.CountAsync(_election.Id, _carol, Admin, Key)).Error.Should().Be("not-requester");
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync()).ConsumedAt.Should().BeNull();
        elections.Verify(x => x.CountAsync(_election.Id, Key, It.IsAny<CancellationToken>()), Times.Never);
        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Results.Should().BeEquivalentTo(Counted);
    }

    [Test]
    public async Task Count_ThatFails_GivesTheApprovalBack()
    {
        var (service, elections) = await CountingAsync();
        elections.Setup(x => x.CountAsync(_election.Id, "wrong", It.IsAny<CancellationToken>())).ReturnsAsync(((IReadOnlyList<ElectionResultDto>?)null, "wrong-key"));
        var pending = (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending!;
        await service.ApproveAsync(pending.Id, _bob, Admin);

        (await service.CountAsync(_election.Id, _alice, Admin, "wrong")).Error.Should().Be("wrong-key");
        var given = await _context.ElectionApprovals.AsNoTracking().SingleAsync();
        given.ConsumedAt.Should().BeNull();
        given.ExecutedByUserId.Should().BeNull();
        (await _context.ActivityLogs.AnyAsync(x => x.ActivityType == Constants.Elections.CountRunAuditType)).Should().BeFalse();
        (await service.CountAsync(_election.Id, _carol, Admin, Key)).Results.Should().NotBeNull();
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync()).ExecutedByUserId.Should().Be(_carol);
    }

    [Test]
    public async Task Count_CannotBeApprovedOutsideCounting()
    {
        var (service, _) = await CountingAsync();
        var pending = (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending!;
        await _context.Elections.Where(x => x.Id == _election.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Phase, ElectionPhase.Polling));

        (await service.ApproveAsync(pending.Id, _bob, Admin)).Error.Should().Be("not-ready");
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync()).ApprovedAt.Should().BeNull();
    }

    [Test]
    public async Task Count_RunsAtOnce_WhenItIsNotATwoPersonStep_OrResultsAreStored()
    {
        var (service, elections) = await CountingAsync();
        UseSettings(new ElectionSettingsDto { TwoPersonActions = [nameof(ElectionApprovalAction.Declare)] });

        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Results.Should().BeEquivalentTo(Counted);

        UseSettings(new ElectionSettingsDto());
        elections.Setup(x => x.CountAsync(_election.Id, null, It.IsAny<CancellationToken>())).ReturnsAsync(((IReadOnlyList<ElectionResultDto>?)Counted, (string?)null));
        (await service.CountAsync(_election.Id, _alice, Admin, null)).Results.Should().BeEquivalentTo(Counted);
        _context.ElectionApprovals.Should().BeEmpty();
    }

    [Test]
    public void Count_IsATwoPersonStep_ByDefault()
    {
        new ElectionSettingsDto().TwoPersonActions.Should().Contain(nameof(ElectionApprovalAction.Count));
    }

    // Spec 023 FR-040 (37.13r). Alice asks as SuperAdmin to remove Carol. Bob is the only other live official with Approve.
    private async Task<int> SetUpRevokeAsync(bool bobIsOfficial = true)
    {
        var persona = new ElectionPersona { Name = "Commissioner", GroupName = Constants.Elections.PersonaGroups.ElectionCommission, Description = "d", DeclarationText = "d", Permissions = ElectionPermission.Approve };
        _context.ElectionPersonas.Add(persona);
        await _context.SaveChangesAsync();
        ElectionAppointment Live(int userId) => new() { ElectionId = _election.Id, PersonaId = persona.Id, UserId = userId, DisplayName = "o", Email = $"{userId}@example.com", AppointedByUserId = _alice, AppointedAt = DateTime.UtcNow, AcceptedAt = DateTime.UtcNow, DeclarationSignedAt = DateTime.UtcNow };
        var target = Live(_carol);
        _context.ElectionAppointments.Add(target);
        if (bobIsOfficial)
            _context.ElectionAppointments.Add(Live(_bob));
        await _context.SaveChangesAsync();
        DetachAll();
        return target.Id;
    }

    private async Task<ElectionApprovalDto> RequestRevokeAsync(int appointmentId)
    {
        var (success, error, approval) = await _service.RequestEmergencyRevokeAsync(appointmentId, _alice, SuperAdmin, "Conflict of interest");
        success.Should().BeTrue(error);
        return approval!;
    }

    private Task<List<ActivityLog>> RevokeAuditAsync() =>
        _context.ActivityLogs.AsNoTracking().Where(x => x.ActivityType == Constants.Elections.EmergencyRevokeAuditType).ToListAsync();

    [Test]
    public async Task EmergencyRevoke_OnlySuperAdminMayAsk()
    {
        var target = await SetUpRevokeAsync();

        (await _service.RequestEmergencyRevokeAsync(target, _alice, Admin, "r")).Error.Should().Be("forbidden");
        (await RevokeAuditAsync()).Should().ContainSingle(x => x.Metadata!.Contains(Constants.Elections.EmergencyRevokeOutcomes.RefusedPrefix + "forbidden"));
    }

    [Test]
    public async Task EmergencyRevoke_RefusedWhenNoOtherOfficialHoldsApprove()
    {
        var target = await SetUpRevokeAsync(bobIsOfficial: false);

        (await _service.RequestEmergencyRevokeAsync(target, _alice, SuperAdmin, "r")).Error.Should().Be("no-eligible-approver");
    }

    [Test]
    public async Task EmergencyRevoke_SecondRequestForSameOfficial_IsRefused()
    {
        var target = await SetUpRevokeAsync();
        await RequestRevokeAsync(target);

        (await _service.RequestEmergencyRevokeAsync(target, _alice, SuperAdmin, "again")).Error.Should().Be("already-pending");
    }

    [Test]
    public async Task EmergencyRevoke_SuperAdminWhoIsNotAnOfficial_CannotApprove()
    {
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);
        var dave = new User { Username = "dave", PasswordHash = "x" };
        _context.Users.Add(dave);
        await _context.SaveChangesAsync();

        (await _service.ApproveAsync(approval.Id, dave.Id, SuperAdmin)).Error.Should().Be("not-eligible-approver");
    }

    [Test]
    public async Task EmergencyRevoke_RequesterAndTarget_CannotApprove()
    {
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);

        (await _service.ApproveAsync(approval.Id, _alice, SuperAdmin)).Error.Should().Be("same-person");
        (await _service.ApproveAsync(approval.Id, _carol, Official)).Error.Should().Be("not-eligible-approver");
    }

    [Test]
    [Category("FR-40")]
    public async Task EmergencyRevoke_TargetAndAdmin_CannotReject()
    {
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);
        var dave = new User { Username = "dave", PasswordHash = "x" };
        _context.Users.Add(dave);
        await _context.SaveChangesAsync();

        (await _service.RejectAsync(approval.Id, _carol, Official, "no")).Error.Should().Be("not-eligible-approver");
        (await _service.RejectAsync(approval.Id, dave.Id, Admin, "no")).Error.Should().Be("not-eligible-approver");
        (await _context.ElectionApprovals.AsNoTracking().SingleAsync(x => x.Id == approval.Id)).RejectedAt.Should().BeNull();
    }

    [Test]
    [Category("FR-40")]
    public async Task EmergencyRevoke_RequesterOrEligibleOfficial_MayReject()
    {
        var target = await SetUpRevokeAsync();
        (await _service.RejectAsync((await RequestRevokeAsync(target)).Id, _alice, SuperAdmin, "withdrawn")).Success.Should().BeTrue();
        (await _service.RejectAsync((await RequestRevokeAsync(target)).Id, _bob, Official, "no")).Success.Should().BeTrue();
    }

    [Test]
    [Category("FR-40")]
    public async Task EmergencyRevoke_TargetRevokedMeanwhile_IsNotWrittenOver()
    {
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);
        var earlier = DateTime.UtcNow.AddMinutes(-5);
        await _context.ElectionAppointments.Where(x => x.Id == target).ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, earlier).SetProperty(x => x.RevokedReason, "ordinary"));

        (await _service.ApproveAsync(approval.Id, _bob, Official)).Error.Should().Be("target-not-live");
        (await _context.ElectionAppointments.AsNoTracking().SingleAsync(x => x.Id == target)).RevokedReason.Should().Be("ordinary");
    }

    [Test]
    public async Task EmergencyRevoke_ApprovedByOfficial_RevokesDuringPolling_AndAudits()
    {
        await _context.Elections.Where(x => x.Id == _election.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Phase, ElectionPhase.Polling));
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);

        (await _service.ApproveAsync(approval.Id, _bob, Official)).Success.Should().BeTrue();

        var row = await _context.ElectionAppointments.AsNoTracking().SingleAsync(x => x.Id == target);
        row.RevokedAt.Should().NotBeNull();
        row.RevokedByUserId.Should().Be(_alice);
        row.RevokedReason.Should().Be("Conflict of interest");
        var audit = await RevokeAuditAsync();
        audit.Should().Contain(x => x.Metadata!.Contains(Constants.Elections.EmergencyRevokeOutcomes.Requested));
        audit.Should().Contain(x => x.ActorId == _bob && x.Metadata!.Contains(Constants.Elections.EmergencyRevokeOutcomes.Revoked));
    }

    [Test]
    public async Task EmergencyRevoke_Expired_IsRefusedAndAudited()
    {
        var target = await SetUpRevokeAsync();
        var approval = await RequestRevokeAsync(target);
        await _context.ElectionApprovals.Where(x => x.Id == approval.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));

        (await _service.ApproveAsync(approval.Id, _bob, Official)).Error.Should().Be("expired");
        (await RevokeAuditAsync()).Should().Contain(x => x.Metadata!.Contains(Constants.Elections.EmergencyRevokeOutcomes.Expired));
        (await _context.ElectionAppointments.AsNoTracking().SingleAsync(x => x.Id == target)).RevokedAt.Should().BeNull();
    }

    // 37.13w. The unique index on OpenKey is what stops two requests sent at the same moment.
    private Task<string?> OpenKeyAsync(int approvalId) =>
        _context.ElectionApprovals.AsNoTracking().Where(x => x.Id == approvalId).Select(x => x.OpenKey).SingleAsync();

    [Test]
    public async Task OpenKey_IsUnique_SoARacingSecondRequestCannotBeStored()
    {
        var pending = await RequestPublishAsync();
        (await OpenKeyAsync(pending.Id)).Should().Be(ElectionApproval.KeyFor(_election.Id, ElectionApprovalAction.Publish));

        _context.ElectionApprovals.Add(new ElectionApproval { ElectionId = _election.Id, Action = ElectionApprovalAction.Publish, RequestedByUserId = _bob, RequestedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), OpenKey = ElectionApproval.KeyFor(_election.Id, ElectionApprovalAction.Publish) });

        await FluentActions.Awaiting(() => _context.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
    }

    [Test]
    public async Task ExpiredRequest_GivesUpItsKey_ToTheNewRequest()
    {
        var old = await RequestPublishAsync();
        await _context.ElectionApprovals.Where(x => x.Id == old.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));

        var fresh = await RequestPublishAsync();

        (await OpenKeyAsync(old.Id)).Should().BeNull();
        (await OpenKeyAsync(fresh.Id)).Should().NotBeNull();
    }

    [Test]
    public async Task RejectedRequest_FreesTheKey()
    {
        var pending = await RequestPublishAsync();
        (await _service.RejectAsync(pending.Id, _bob, Admin, "no")).Success.Should().BeTrue();

        (await OpenKeyAsync(pending.Id)).Should().BeNull();
        await RequestPublishAsync();
    }

    [Test]
    public async Task StepThatRan_FreesTheKey()
    {
        var pending = await RequestPublishAsync();
        (await _service.ApproveAsync(pending.Id, _bob, Admin)).Success.Should().BeTrue();

        (await OpenKeyAsync(pending.Id)).Should().BeNull();
    }

    [Test]
    public async Task ApprovedCount_KeepsItsKey_UntilItRuns()
    {
        var (service, elections) = await CountingAsync();
        elections.Setup(x => x.CountAsync(_election.Id, "wrong", It.IsAny<CancellationToken>())).ReturnsAsync(((IReadOnlyList<ElectionResultDto>?)null, "wrong-key"));
        var pending = (await service.CountAsync(_election.Id, _alice, Admin, Key)).Pending!;
        await service.ApproveAsync(pending.Id, _bob, Admin);
        (await OpenKeyAsync(pending.Id)).Should().NotBeNull();

        await service.CountAsync(_election.Id, _alice, Admin, "wrong");
        (await OpenKeyAsync(pending.Id)).Should().NotBeNull();

        (await service.CountAsync(_election.Id, _alice, Admin, Key)).Results.Should().NotBeNull();
        (await OpenKeyAsync(pending.Id)).Should().BeNull();
    }

    // 37.13x. An emergency revoke that nobody approves still gets one expired row.
    private Task<List<ActivityLog>> ExpiredAuditAsync() =>
        _context.ActivityLogs.AsNoTracking().Where(x => x.ActivityType == Constants.Elections.EmergencyRevokeAuditType && x.Metadata!.Contains("\"outcome\":\"" + Constants.Elections.EmergencyRevokeOutcomes.Expired + "\"")).ToListAsync();

    private async Task<ElectionApprovalDto> ExpiredRevokeAsync()
    {
        var approval = await RequestRevokeAsync(await SetUpRevokeAsync());
        await _context.ElectionApprovals.Where(x => x.Id == approval.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
        return approval;
    }

    [Test]
    [Category("FR-40")]
    public async Task Sweep_AuditsAnUntouchedExpiredRevoke_Once()
    {
        var approval = await ExpiredRevokeAsync();

        (await _service.AuditExpiredRevokesAsync()).Should().Be(1);
        (await _service.AuditExpiredRevokesAsync()).Should().Be(0);

        var row = (await ExpiredAuditAsync()).Should().ContainSingle().Subject;
        row.Metadata.Should().Contain($"\"approvalId\":{approval.Id}").And.Contain("\"expiredAt\":\"");
        (await OpenKeyAsync(approval.Id)).Should().BeNull();
    }

    [Test]
    [Category("FR-40")]
    public async Task Sweep_PayloadThatWillNotParse_StillWritesTheExpiredRow()
    {
        var approval = await ExpiredRevokeAsync();
        await _context.ElectionApprovals.Where(x => x.Id == approval.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.PayloadJson, "{not json"));

        (await _service.AuditExpiredRevokesAsync()).Should().Be(1);

        (await ExpiredAuditAsync()).Should().ContainSingle(x => x.Metadata!.Contains($"\"approvalId\":{approval.Id}"));
        (await OpenKeyAsync(approval.Id)).Should().BeNull();
    }

    [Test]
    [Category("FR-40")]
    public async Task ApprovingAfterTheSweep_DoesNotWriteASecondExpiredRow()
    {
        var approval = await ExpiredRevokeAsync();
        await _service.AuditExpiredRevokesAsync();

        (await _service.ApproveAsync(approval.Id, _bob, Official)).Error.Should().Be("expired");

        (await ExpiredAuditAsync()).Should().ContainSingle();
    }

    [Test]
    [Category("FR-40")]
    public async Task SweepAfterAnApproveAttempt_DoesNotWriteASecondExpiredRow()
    {
        var approval = await ExpiredRevokeAsync();
        (await _service.ApproveAsync(approval.Id, _bob, Official)).Error.Should().Be("expired");

        (await _service.AuditExpiredRevokesAsync()).Should().Be(0);
        (await ExpiredAuditAsync()).Should().ContainSingle();
    }

    [Test]
    [Category("FR-40")]
    public async Task NewRevokeRequest_AuditsTheExpiredOne_BeforeTakingItsKey()
    {
        var approval = await ExpiredRevokeAsync();
        var carol = await _context.ElectionAppointments.AsNoTracking().SingleAsync(x => x.UserId == _carol);

        var (success, error, fresh) = await _service.RequestEmergencyRevokeAsync(carol.Id, _alice, SuperAdmin, "again");

        success.Should().BeTrue(error);
        (await ExpiredAuditAsync()).Should().ContainSingle(x => x.Metadata!.Contains($"\"approvalId\":{approval.Id}"));
        (await OpenKeyAsync(fresh!.Id)).Should().NotBeNull();
    }

    [Test]
    [Category("FR-40")]
    public async Task HostedSweep_RunOnce_UsesAScopedApprovalService()
    {
        await ExpiredRevokeAsync();
        using var provider = new ServiceCollection().AddScoped<IElectionApprovalService>(_ => _service).BuildServiceProvider();
        var sweep = new ExpiredRevokeSweep(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<ExpiredRevokeSweep>.Instance);

        await sweep.RunOnceAsync(CancellationToken.None);

        (await ExpiredAuditAsync()).Should().ContainSingle();
    }
}
