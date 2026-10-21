using System.Text.Json;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 (37.14m): the extra records the printed forms need. Real service against the test
// database, so each test also checks the audit row it should leave.
[TestFixture]
[Category("FR-25")]
public sealed class ElectionFormServiceTests : TestBase
{
    private ElectionFormService _service = null!;
    private Election _election = null!;
    private ElectionSeat _seat = null!;
    private Nomination _first = null!;
    private Nomination _second = null!;
    private int _user;

    [SetUp]
    public async Task SetUpFixture()
    {
        _service = new ElectionFormService(_context);

        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        var user = new User { Username = "official", PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _user = user.Id;

        _election = new Election
        {
            Title = "Forms Election",
            ECPeriodId = period.Id,
            NominationOpensOn = DateTime.UtcNow.AddDays(-3),
            NominationClosesOn = DateTime.UtcNow.AddDays(1),
            PollingOpensOn = DateTime.UtcNow.AddDays(5),
            PollingClosesOn = DateTime.UtcNow.AddDays(6)
        };
        _seat = new ElectionSeat { Position = EcPositionIds.President };
        _election.Seats.Add(_seat);
        _context.Elections.Add(_election);
        await _context.SaveChangesAsync();

        var a = await AddMemberAsync("Alice", 1);
        var b = await AddMemberAsync("Bob", 2);
        var p = await AddMemberAsync("Pat", 3);
        var s = await AddMemberAsync("Sam", 4);
        _first = await AddNominationAsync(a, p, s);
        _second = await AddNominationAsync(b, p, s);
    }

    private async Task<int> AddMemberAsync(string name, int n)
    {
        var member = new Member
        {
            FullName = name,
            FatherName = "F",
            MotherName = "Mo",
            Email = $"m{n}@example.com",
            NID = "N" + n,
            MobileNo = "0170" + n.ToString("D7"),
            PresentAddress = "A",
            PermanentAddress = "A",
            EmergencyContactName = "E",
            EmergencyContactRelation = "R",
            EmergencyContactPhone = "0"
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member.Id;
    }

    private async Task<Nomination> AddNominationAsync(int candidate, int proposer, int seconder)
    {
        var nomination = new Nomination
        {
            ElectionId = _election.Id,
            ElectionSeatId = _seat.Id,
            CandidateMemberId = candidate,
            ProposerMemberId = proposer,
            SeconderMemberId = seconder,
            Statement = "s",
            Status = NominationStatus.Accepted,
            SubmittedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.Nominations.Add(nomination);
        await _context.SaveChangesAsync();
        return nomination;
    }

    private async Task SetPhaseAsync(ElectionPhase phase, DateTime? recountWindowEndsOn = null)
    {
        _election.Phase = phase;
        _election.RecountWindowEndsOn = recountWindowEndsOn;
        await _context.SaveChangesAsync();
    }

    private async Task<Election> ReloadAsync() => await _context.Elections.AsNoTracking().SingleAsync(x => x.Id == _election.Id);

    private Task<List<string>> AuditActionsAsync() =>
        _context.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == _election.Id).OrderBy(x => x.Seq).Select(x => x.Action).ToListAsync();

    private async Task DeclareAsync()
    {
        await SetPhaseAsync(ElectionPhase.Declared);
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 10, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 8 });
        await _context.SaveChangesAsync();
    }

    private async Task<int> DeclaredRecountAsync(bool? allowed = null)
    {
        await DeclareAsync();
        var row = new ElectionRecountRequest
        {
            ElectionId = _election.Id,
            ElectionSeatId = _seat.Id,
            ApplicantName = "Bob",
            Grounds = "Close margin",
            ReceivedAt = DateTime.UtcNow.AddHours(-1),
            RecordedByUserId = _user,
            Allowed = allowed,
            DecidedAt = allowed == null ? null : DateTime.UtcNow,
            DecidedByUserId = allowed == null ? null : _user
        };
        _context.ElectionRecountRequests.Add(row);
        await _context.SaveChangesAsync();
        return row.Id;
    }

    private static RecordRecountRequest RecountRequest(int seatId, DateTime? receivedAt) => new()
    {
        ElectionSeatId = seatId,
        ApplicantName = "  Bob  ",
        Grounds = " Close margin ",
        Explanation = "  ",
        ReceivedAt = receivedAt
    };

    [Test]
    public async Task UpdateFormDetails_SavesTrimmedValues_AndAudits()
    {
        var request = new UpdateElectionFormDetailsRequest
        {
            FinalListOn = _election.NominationClosesOn.AddDays(1),
            CampaignOpensOn = _election.NominationClosesOn,
            CampaignClosesOn = _election.PollingOpensOn,
            SilenceStartsAt = _election.PollingOpensOn.AddHours(-12),
            Venue = "  Main hall  ",
            NoticeReference = " ",
            RecountWindowEndsOn = _election.PollingClosesOn.AddDays(2)
        };

        var (error, _) = await _service.UpdateFormDetailsAsync(_election.Id, request, _user);

        error.Should().BeNull();
        var saved = await ReloadAsync();
        saved.Venue.Should().Be("Main hall");
        saved.NoticeReference.Should().BeNull();
        saved.SilenceStartsAt.Should().BeCloseTo(request.SilenceStartsAt!.Value, TimeSpan.FromSeconds(1));
        saved.RecountWindowEndsOn.Should().BeCloseTo(request.RecountWindowEndsOn!.Value, TimeSpan.FromSeconds(1));
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.FormDetailsChanged);
    }

    [Test]
    public async Task UpdateFormDetails_AuditKeepsTheOldValuesBesideTheNew()
    {
        _election.Venue = "Old hall";
        await _context.SaveChangesAsync();

        await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { Venue = "Main hall" }, _user);

        var json = await _context.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == _election.Id).Select(x => x.DetailJson).SingleAsync();
        using var detail = JsonDocument.Parse(json);
        detail.RootElement.GetProperty("from").GetProperty("venue").GetString().Should().Be("Old hall");
        detail.RootElement.GetProperty("to").GetProperty("venue").GetString().Should().Be("Main hall");
    }

    [Test]
    public async Task DecideRecount_WhenTheSaveClashes_ReturnsBusyAndDropsTheChange()
    {
        var id = await DeclaredRecountAsync();
        var audit = new Mock<IElectionAuditWriter>();
        audit.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateException("clash"));
        var service = new ElectionFormService(_context, audit.Object);

        var error = await service.DecideRecountAsync(_election.Id, id, new DecideRecountRequest { Allowed = true }, _user);

        error.Should().Be(Constants.Elections.Replies.Busy);
        _context.ChangeTracker.Entries().Should().BeEmpty();
        (await _context.ElectionRecountRequests.AsNoTracking().SingleAsync(x => x.Id == id)).Allowed.Should().BeNull();
    }

    [Test]
    public async Task UpdateFormDetails_RefusesDatesOutOfOrder()
    {
        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { FinalListOn = _election.NominationClosesOn.AddDays(-1) }, _user))
            .Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { SilenceStartsAt = _election.PollingClosesOn }, _user))
            .Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { RecountWindowEndsOn = _election.PollingClosesOn.AddMinutes(-1) }, _user))
            .Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { CampaignOpensOn = DateTime.UtcNow.AddDays(2), CampaignClosesOn = DateTime.UtcNow.AddDays(1) }, _user))
            .Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await AuditActionsAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task UpdateFormDetails_FinalListOnTheDayNominationsClose_IsAllowed()
    {
        var sameDay = _election.NominationClosesOn.Date;

        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest { FinalListOn = sameDay }, _user))
            .Error.Should().BeNull();
        (await ReloadAsync()).FinalListOn.Should().Be(sameDay);
    }

    [Test]
    public async Task UpdateFormDetails_ArchivedOrMissing_IsRefused()
    {
        (await _service.UpdateFormDetailsAsync(9999, new UpdateElectionFormDetailsRequest(), _user)).Error.Should().Be(Constants.Elections.Replies.NotFound);
        await SetPhaseAsync(ElectionPhase.Archived);
        (await _service.UpdateFormDetailsAsync(_election.Id, new UpdateElectionFormDetailsRequest(), _user)).Error.Should().Be(Constants.Elections.Replies.WrongPhase);
    }

    [Test]
    [Category("FR-21")]
    public async Task RecordConsent_StoresTime_AndAudits()
    {
        var at = DateTime.UtcNow.AddHours(-2);

        (await _service.RecordConsentAsync(_election.Id, _first.Id, at, _user)).Should().BeNull();

        (await _context.Nominations.AsNoTracking().SingleAsync(x => x.Id == _first.Id)).ConsentAt.Should().BeCloseTo(at, TimeSpan.FromSeconds(1));
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.ConsentRecorded);
    }

    [Test]
    [Category("FR-21")]
    public async Task RecordConsent_FutureTime_PollingPhase_OrOtherElection_IsRefused()
    {
        (await _service.RecordConsentAsync(_election.Id, _first.Id, DateTime.UtcNow.AddHours(1), _user)).Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.RecordConsentAsync(_election.Id + 1, _first.Id, DateTime.UtcNow, _user)).Should().Be(Constants.Elections.Replies.NotFound);
        await SetPhaseAsync(ElectionPhase.Polling);
        (await _service.RecordConsentAsync(_election.Id, _first.Id, DateTime.UtcNow, _user)).Should().Be(Constants.Elections.Replies.WrongPhase);
        (await AuditActionsAsync()).Should().BeEmpty();
    }

    [Test]
    [Category("FR-21")]
    public async Task Nominations_ConsentTime_OnlyOnTheOfficialsList()
    {
        (await _service.RecordConsentAsync(_election.Id, _first.Id, DateTime.UtcNow.AddHours(-2), _user)).Should().BeNull();
        var elections = new ElectionService(_context);

        (await elections.GetNominationsAsync(_election.Id)).Should().HaveCount(2).And.OnlyContain(x => x.ConsentAt == null);
        var official = await elections.GetNominationsWithConsentAsync(_election.Id);
        official.Single(x => x.Id == _first.Id).ConsentAt.Should().NotBeNull();
        official.Single(x => x.Id == _second.Id).ConsentAt.Should().BeNull();
    }

    [Test]
    public async Task ScrutinyCheck_OutsideScrutiny_IsRefused()
    {
        var request = new SetScrutinyCheckRequest { Item = ScrutinyCheckItem.DocumentsComplete, IsChecked = true };

        (await _service.SetScrutinyCheckAsync(_election.Id, _first.Id, request, _user)).Should().Be(Constants.Elections.Replies.WrongPhase);
        (await _service.GetScrutinyChecksAsync(_election.Id, _first.Id)).Should().BeEmpty();
    }

    [Test]
    public async Task ScrutinyCheck_TickThenUntick_KeepsOneRow()
    {
        await SetPhaseAsync(ElectionPhase.Scrutiny);
        var item = ScrutinyCheckItem.NominationFeePaid;

        (await _service.SetScrutinyCheckAsync(_election.Id, _first.Id, new SetScrutinyCheckRequest { Item = item, IsChecked = true }, _user)).Should().BeNull();
        (await _service.SetScrutinyCheckAsync(_election.Id, _first.Id, new SetScrutinyCheckRequest { Item = item, IsChecked = false }, _user)).Should().BeNull();

        var checks = await _service.GetScrutinyChecksAsync(_election.Id, _first.Id);
        checks.Should().ContainSingle().Which.Should().Match<ScrutinyCheckDto>(c => c.Item == item && !c.IsChecked && c.SetByUserId == _user);
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.ScrutinyChecked, Constants.Elections.AuditActions.ScrutinyChecked);
    }

    [Test]
    public async Task ScrutinyCheck_UnknownNominationOrItem_IsRefused()
    {
        await SetPhaseAsync(ElectionPhase.Scrutiny);

        (await _service.GetScrutinyChecksAsync(_election.Id, 9999)).Should().BeNull();
        (await _service.SetScrutinyCheckAsync(_election.Id, 9999, new SetScrutinyCheckRequest { Item = ScrutinyCheckItem.VotingMember }, _user))
            .Should().Be(Constants.Elections.Replies.NotFound);
        (await _service.SetScrutinyCheckAsync(_election.Id, _first.Id, new SetScrutinyCheckRequest { Item = (ScrutinyCheckItem)99 }, _user))
            .Should().Be(Constants.Elections.Replies.Invalid);
    }

    [Test]
    [Category("FR-20")]
    public async Task RecordRecount_AfterDeclare_StoresTrimmedRequest()
    {
        await DeclareAsync();

        var (recount, error, _) = await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, DateTime.UtcNow.AddMinutes(-5)), _user);

        error.Should().BeNull();
        recount!.ApplicantName.Should().Be("Bob");
        recount.Grounds.Should().Be("Close margin");
        recount.Explanation.Should().BeNull();
        recount.Allowed.Should().BeNull();
        (await _service.ListRecountsAsync(_election.Id)).Should().ContainSingle(x => x.Id == recount.Id);
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.RecountRequested);
    }

    [Test]
    [Category("FR-20")]
    public async Task RecordRecount_BeforeDeclare_LateOrWrongSeat_IsRefused()
    {
        (await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, DateTime.UtcNow), _user)).Error.Should().Be(Constants.Elections.Replies.WrongPhase);

        await DeclareAsync();
        await SetPhaseAsync(ElectionPhase.Declared, recountWindowEndsOn: DateTime.UtcNow.AddHours(-2));
        (await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, DateTime.UtcNow.AddHours(-1)), _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);

        await SetPhaseAsync(ElectionPhase.Declared);
        (await _service.RecordRecountAsync(_election.Id, RecountRequest(9999, DateTime.UtcNow), _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, DateTime.UtcNow.AddHours(1)), _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, null), _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _context.ElectionRecountRequests.CountAsync()).Should().Be(0);
    }

    [Test]
    [Category("FR-20")]
    public async Task RecordRecount_SeatWithNoDeclaredResult_IsRefused()
    {
        await SetPhaseAsync(ElectionPhase.Declared);

        var (_, error, detail) = await _service.RecordRecountAsync(_election.Id, RecountRequest(_seat.Id, DateTime.UtcNow.AddMinutes(-5)), _user);

        error.Should().Be(Constants.Elections.Replies.Invalid);
        detail.Should().Contain("no declared result");
        (await _context.ElectionRecountRequests.CountAsync()).Should().Be(0);
    }

    [Test]
    [Category("FR-20")]
    public async Task DecideRecount_RefusalNeedsReason_AndDecidesOnce()
    {
        var id = await DeclaredRecountAsync();

        (await _service.DecideRecountAsync(_election.Id, id, new DecideRecountRequest { Allowed = false, Reason = " " }, _user)).Should().Be(Constants.Elections.Replies.Invalid);
        (await _service.DecideRecountAsync(_election.Id, id, new DecideRecountRequest { Allowed = false, Reason = "Margin above threshold" }, _user)).Should().BeNull();
        (await _service.DecideRecountAsync(_election.Id, id, new DecideRecountRequest { Allowed = true }, _user)).Should().Be(Constants.Elections.Replies.AlreadyRecorded);

        var row = (await _service.ListRecountsAsync(_election.Id)).Single();
        row.Allowed.Should().BeFalse();
        row.DecisionReason.Should().Be("Margin above threshold");
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.RecountDecided);
    }

    [Test]
    [Category("FR-20")]
    public async Task RecountFigures_StoredBesideResult_WithoutChangingIt()
    {
        var id = await DeclaredRecountAsync(allowed: true);
        var figures = new List<RecountFigureInput> { new() { NominationId = _first.Id, VoteCount = 9 }, new() { NominationId = _second.Id, VoteCount = 9 } };

        (await _service.RecordRecountFiguresAsync(_election.Id, id, figures, _user)).Error.Should().BeNull();
        (await _service.RecordRecountFiguresAsync(_election.Id, id, figures, _user)).Error.Should().Be(Constants.Elections.Replies.AlreadyRecorded);

        var row = (await _service.ListRecountsAsync(_election.Id)).Single();
        row.RecountedAt.Should().NotBeNull();
        row.Figures.Select(f => f.VoteCount).Should().Equal(9, 9);
        (await _context.ElectionResults.AsNoTracking().OrderBy(x => x.NominationId).Select(x => x.VoteCount).ToListAsync()).Should().Equal(10, 8);
        (await AuditActionsAsync()).Should().Equal(Constants.Elections.AuditActions.RecountCounted);
    }

    [Test]
    [Category("FR-20")]
    public async Task RecountFigures_MustMatchCandidates_AndNeedAllowedRecount()
    {
        var pending = await DeclaredRecountAsync();
        var one = new List<RecountFigureInput> { new() { NominationId = _first.Id, VoteCount = 9 } };
        (await _service.RecordRecountFiguresAsync(_election.Id, pending, one, _user)).Error.Should().Be(Constants.Elections.Replies.WrongPhase);

        var allowed = new ElectionRecountRequest { ElectionId = _election.Id, ElectionSeatId = _seat.Id, ApplicantName = "Al", Grounds = "g", ReceivedAt = DateTime.UtcNow, RecordedByUserId = _user, Allowed = true };
        _context.ElectionRecountRequests.Add(allowed);
        await _context.SaveChangesAsync();

        (await _service.RecordRecountFiguresAsync(_election.Id, allowed.Id, one, _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        var duplicate = new List<RecountFigureInput> { new() { NominationId = _first.Id, VoteCount = 9 }, new() { NominationId = _first.Id, VoteCount = 9 } };
        (await _service.RecordRecountFiguresAsync(_election.Id, allowed.Id, duplicate, _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        var negative = new List<RecountFigureInput> { new() { NominationId = _first.Id, VoteCount = -1 }, new() { NominationId = _second.Id, VoteCount = 9 } };
        (await _service.RecordRecountFiguresAsync(_election.Id, allowed.Id, negative, _user)).Error.Should().Be(Constants.Elections.Replies.Invalid);
        (await _context.ElectionRecountFigures.CountAsync()).Should().Be(0);
    }
}
