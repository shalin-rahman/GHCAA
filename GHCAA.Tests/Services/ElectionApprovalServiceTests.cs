using System.Security.Cryptography;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
        _service = new ElectionApprovalService(_context, new ElectionService(_context), new ElectionAccessService(_context, _orgConfig.Object), _orgConfig.Object, NullLogger<ElectionApprovalService>.Instance);

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
}
