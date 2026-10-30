using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 FR-025 (37.1q): election forms fill themselves from stored records.
[TestFixture]
[Category("FR-25")]
public sealed class ElectionDocumentServiceTests : TestBase
{
    private ElectionDocumentService _service = null!;
    private Mock<IOrgConfigService> _config = null!;
    private Election _election = null!;
    private ElectionSeat _seat = null!;
    private Nomination _first = null!;
    private Nomination _second = null!;

    [SetUp]
    public async Task SetUpFixture()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        _config = new Mock<IOrgConfigService>();
        _config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        _service = new ElectionDocumentService(_context, _config.Object);
        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);

        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        _election = new Election
        {
            Title = "Forms Election",
            ECPeriodId = period.Id,
            NominationOpensOn = DateTime.UtcNow,
            NominationClosesOn = DateTime.UtcNow.AddDays(1),
            PollingOpensOn = DateTime.UtcNow.AddDays(2),
            PollingClosesOn = DateTime.UtcNow.AddDays(3)
        };
        _seat = new ElectionSeat { Position = EcPositionIds.President };
        _election.Seats.Add(_seat);
        _context.Elections.Add(_election);
        await _context.SaveChangesAsync();

        var a = await AddMemberAsync("Alice Candidate", 1);
        var b = await AddMemberAsync("Bob Candidate", 2);
        var p = await AddMemberAsync("Pat Proposer", 3);
        var s = await AddMemberAsync("Sam Seconder", 4);
        _first = await AddNominationAsync(a, p, s, 1);
        _second = await AddNominationAsync(b, p, s, 2);
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

    private async Task<Nomination> AddNominationAsync(int candidate, int proposer, int seconder, int order)
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
            SubmittedAt = new DateTime(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc),
            BallotOrder = order
        };
        _context.Nominations.Add(nomination);
        await _context.SaveChangesAsync();
        return nomination;
    }

    [Test]
    public async Task ER27_CarriesPerCandidateCounts()
    {
        // Three ballots: one spoiled, one for Alice, one that left the seat empty.
        var cast = Enumerable.Range(0, 3).Select(i => new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = i == 2 }).ToList();
        _context.Ballots.AddRange(cast);
        _context.BallotVotes.Add(new BallotVote { Id = Guid.NewGuid(), BallotId = cast[0].Id, ElectionSeatId = _seat.Id, NominationId = _first.Id });
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 1, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 0 });
        await _context.SaveChangesAsync();

        var tables = await _service.LoadTablesAsync(_election.Id, "ER-27");

        tables[0].Rows.Single().Should().Equal("3", "1", "2");
        tables[1].Rows.Select(r => string.Join(":", r)).Should().Equal("Alice Candidate:1", "Bob Candidate:0", "Abstain:1");
    }

    [Test]
    [Category("FR-21")]
    public async Task ER11_PrintsConsentTime_OrNotRecorded()
    {
        _first.ConsentAt = new DateTime(2026, 10, 2, 14, 5, 0, DateTimeKind.Utc);
        await _context.SaveChangesAsync();

        var tables = await _service.LoadTablesAsync(_election.Id, "er-11");

        tables.Single().Rows.Select(r => $"{r[0]}:{r[2]}").Should().Equal("Alice Candidate:2026-10-02 14:05 UTC", "Bob Candidate:Not recorded");
    }

    [Test]
    public async Task ER10_ListsProposerAndSeconder()
    {
        var row = (await _service.LoadTablesAsync(_election.Id, "ER-10")).Single().Rows[0];

        row.Should().Contain(new[] { "Alice Candidate", "Pat Proposer", "Sam Seconder" });
    }

    [Test]
    public async Task ER18_ListsOnlyWithdrawnNominations()
    {
        _second.Status = NominationStatus.Withdrawn;
        _second.WithdrawnAt = new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc);
        await _context.SaveChangesAsync();

        (await _service.LoadTablesAsync(_election.Id, "ER-18")).Single().Rows.Select(r => r[0]).Should().Equal("Bob Candidate");
    }

    [Test]
    public async Task ER27C_ShowsOutcomePerSeat()
    {
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 2 });
        await _context.SaveChangesAsync();

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-27C"))[1].Rows;

        rows.Select(r => r[^1]).Should().Equal("Elected", "Not elected");
    }

    // 37.14d. The printed result sheet carries the same head hash as the public board.
    [Test]
    public async Task ER27C_PrintsTheAuditChainHead()
    {
        var before = (await _service.LoadTablesAsync(_election.Id, "ER-27C"))[0].Rows;
        before.Should().ContainSingle(r => r[0] == "Audit chain head").Which[1].Should().Be("Not declared");

        var writer = new ElectionAuditWriter(_context);
        await writer.AppendAsync(_election.Id, GHCAA.Domain.Constants.Elections.AuditActions.Declared, null, null);
        await writer.SaveAsync();
        _election.Phase = ElectionPhase.Declared;
        await _context.SaveChangesAsync();
        var head = _context.ElectionAuditEntries.Single().Hash;

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-27C"))[0].Rows;

        rows.Should().ContainSingle(r => r[0] == "Audit chain head").Which[1].Should().Be(head);
        (await _service.LoadTablesAsync(_election.Id, "ER-29"))[0].Rows.Should().NotContain(r => r[0] == "Audit chain head");
    }

    // 37.14g. ER-25 states the turnout, the seal key and whether the audit trail still checks out.
    [Test]
    public async Task ER25_CarriesThePollRecord()
    {
        _election.BallotKeyFingerprint = "ab:cd:ef";
        _context.VoterRolls.AddRange(
            new VoterRoll { ElectionId = _election.Id, MemberId = _first.CandidateMemberId, IsEligible = true, FrozenAt = DateTime.UtcNow, VotedAt = DateTime.UtcNow.Date },
            new VoterRoll { ElectionId = _election.Id, MemberId = _second.CandidateMemberId, IsEligible = true, FrozenAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var record = (await _service.LoadTablesAsync(_election.Id, "ER-25"))[0].Rows.ToDictionary(r => r[0], r => r[1]);

        record["Eligible voters"].Should().Be("2");
        record["Voters who cast a ballot"].Should().Be("1");
        record["Ballot seal key fingerprint"].Should().Be("ab:cd:ef");
        record["Audit trail"].Should().StartWith("Intact");
    }

    // 37.14p. ER-04 lists the live Election Commission only, with the membership number looked up.
    [Test]
    public async Task ER04_ListsCommissionAppointments()
    {
        var commission = await AddPersonaAsync("Chief Commissioner", Constants.Elections.PersonaGroups.ElectionCommission);
        var official = await AddPersonaAsync("Polling Officer", Constants.Elections.PersonaGroups.Officials);
        await AddAppointmentAsync(commission, "Cara Commissioner", _first.CandidateMemberId);
        await AddAppointmentAsync(commission, "Rex Revoked", null, revoked: true);
        await AddAppointmentAsync(official, "Olly Official", null);
        _context.Members.Single(m => m.Id == _first.CandidateMemberId).MembershipNumber = "M-001";
        await _context.SaveChangesAsync();

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-04"))[0].Rows;

        rows.Should().ContainSingle();
        rows[0].Take(3).Should().Equal("Cara Commissioner", "M-001", "Chief Commissioner");
        rows[0][4].Should().NotBe("Not yet");
    }

    // 37.14p. ER-05 lists every live appointee. An outsider has no membership number and an
    // unsigned declaration reads "Not yet".
    [Test]
    public async Task ER05_ListsEveryDeclaration()
    {
        var commission = await AddPersonaAsync("Chief Commissioner", Constants.Elections.PersonaGroups.ElectionCommission);
        var official = await AddPersonaAsync("Polling Officer", Constants.Elections.PersonaGroups.Officials);
        await AddAppointmentAsync(commission, "Cara Commissioner", _first.CandidateMemberId);
        await AddAppointmentAsync(official, "Olly Official", null, signed: false);

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-05"))[0].Rows.ToDictionary(r => r[0]);

        rows.Should().HaveCount(2);
        rows["Olly Official"][1].Should().Be("Not a member");
        rows["Olly Official"][3].Should().Be("Not yet");
        rows["Cara Commissioner"][3].Should().NotBe("Not yet");
    }

    // 37.14p. ER-23 and ER-24 carry the stored poll facts. ER-24 counts every ballot, spoiled ones too.
    [Test]
    public async Task ER23_ER24_CarryThePollFacts()
    {
        _election.Venue = "Main Hall";
        _context.VoterRolls.Add(new VoterRoll { ElectionId = _election.Id, MemberId = _first.CandidateMemberId, IsEligible = true, FrozenAt = DateTime.UtcNow });
        _context.Ballots.AddRange(
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id },
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = true });
        await _context.SaveChangesAsync();

        var opening = (await _service.LoadTablesAsync(_election.Id, "ER-23"))[0].Rows.ToDictionary(r => r[0], r => r[1]);
        var closing = (await _service.LoadTablesAsync(_election.Id, "ER-24"))[0].Rows.ToDictionary(r => r[0], r => r[1]);

        opening["Venue"].Should().Be("Main Hall");
        opening["Registered voters"].Should().Be("1");
        opening["Ballot seal key fingerprint"].Should().Be("No key registered");
        closing["Ballots cast, spoiled included"].Should().Be("2");
        closing["Spoiled ballots"].Should().Be("1");
        closing["Polling closed"].Should().StartWith("Not yet; closes").And.EndWith("UTC");
        closing["Counts"].Should().Be("So far; polling is still open");
    }

    [Test]
    public async Task ER24_AfterClose_PrintsCloseTimeAndNoRunningTotalNote()
    {
        _election.PollingOpensOn = DateTime.UtcNow.AddDays(-2);
        _election.PollingClosesOn = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync();

        var closing = (await _service.LoadTablesAsync(_election.Id, "ER-24"))[0].Rows.ToDictionary(r => r[0], r => r[1]);

        closing["Polling closed"].Should().NotStartWith("Not yet").And.EndWith("UTC");
        closing.Should().NotContainKey("Counts");
    }

    [Test]
    public async Task ER23_NoVenue_SaysNotRecorded()
    {
        var opening = (await _service.LoadTablesAsync(_election.Id, "ER-23"))[0].Rows.ToDictionary(r => r[0], r => r[1]);

        opening["Venue"].Should().Be("Not recorded");
        opening["Registered voters"].Should().Be("0");
    }

    // An unset poll date is year 0001 in the database. It must read "Not recorded", never 0001-01-01.
    [Test]
    public async Task UnsetPollDates_PrintNotRecorded_NeverYearOne()
    {
        _election.PollingOpensOn = default;
        _election.PollingClosesOn = default;
        await _context.SaveChangesAsync();

        var opening = (await _service.LoadTablesAsync(_election.Id, "ER-23"))[0].Rows.ToDictionary(r => r[0], r => r[1]);
        var closing = (await _service.LoadTablesAsync(_election.Id, "ER-24"))[0].Rows.ToDictionary(r => r[0], r => r[1]);
        var record = (await _service.LoadTablesAsync(_election.Id, "ER-25"))[0].Rows.ToDictionary(r => r[0], r => r[1]);
        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-01");

        opening["Polling opened"].Should().Be("Not recorded");
        closing["Polling closed"].Should().Be("Not recorded");
        record["Polling opened"].Should().Be("Not recorded");
        fields.Should().NotContainKeys("Election Date", "Polling Time");
        opening.Values.Concat(closing.Values).Concat(record.Values).Concat(fields.Values).Should().NotContain(v => v.Contains("0001"));
    }

    // Access ends some days after the declaration. An accepted appointment stays on the record
    // after that; one that lapsed before anyone accepted it never served and drops off.
    [Test]
    public async Task ER04_ER05_KeepAcceptedAfterExpiry_DropLapsedUnaccepted()
    {
        var commission = await AddPersonaAsync("Chief Commissioner", Constants.Elections.PersonaGroups.ElectionCommission);
        var past = DateTime.UtcNow.AddDays(-1);
        await AddAppointmentAsync(commission, "Ada Accepted", null, expiresAt: past);
        await AddAppointmentAsync(commission, "Lou Lapsed", null, signed: false, expiresAt: past);
        await AddAppointmentAsync(commission, "Pam Pending", null, signed: false, expiresAt: DateTime.UtcNow.AddDays(5));

        var er04 = (await _service.LoadTablesAsync(_election.Id, "ER-04"))[0].Rows.ToDictionary(r => r[0]);
        var er05 = (await _service.LoadTablesAsync(_election.Id, "ER-05"))[0].Rows.ToDictionary(r => r[0]);

        er04.Keys.Should().BeEquivalentTo("Ada Accepted", "Pam Pending");
        er05.Keys.Should().BeEquivalentTo("Ada Accepted", "Pam Pending");
        er04["Pam Pending"][4].Should().Be("Not yet");
        er05["Pam Pending"][3].Should().Be("Not yet");
    }

    // 37.14q. Ruled fields on ER-01 take the stored schedule. Unset dates and per-person labels stay blank.
    [Test]
    public async Task FieldValues_ER01_FillTheScheduleOnly()
    {
        _election.Venue = "Main Hall";
        _election.NominationOpensOn = new DateTime(2026, 11, 1, 9, 0, 0, DateTimeKind.Utc);
        _election.PollingOpensOn = new DateTime(2026, 12, 5, 8, 0, 0, DateTimeKind.Utc);
        _election.PollingClosesOn = new DateTime(2026, 12, 5, 16, 0, 0, DateTimeKind.Utc);
        _election.SilenceStartsAt = new DateTime(2026, 12, 4, 8, 0, 0, DateTimeKind.Utc);
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, " er-01 ");

        fields["Election Date"].Should().Be("2026-12-05");
        fields["Polling Time"].Should().Be("08:00 to 16:00 UTC");
        fields["Polling Venue"].Should().Be("Main Hall");
        fields["Nomination Opens"].Should().Be("2026-11-01 09:00 UTC");
        fields["Silence Period"].Should().Be("From 2026-12-04 08:00 UTC");
        fields.Should().NotContainKeys("Scrutiny", "Withdrawal Deadline", "Campaign Period", "Name", "Membership No.");
    }

    [Test]
    public async Task FieldValues_ER08_CountTheRoll()
    {
        _context.VoterRolls.AddRange(
            new VoterRoll { ElectionId = _election.Id, MemberId = _first.CandidateMemberId, IsEligible = true, FrozenAt = DateTime.UtcNow },
            new VoterRoll { ElectionId = _election.Id, MemberId = _second.CandidateMemberId, IsEligible = false, FrozenAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-08");

        fields["Total Members"].Should().Be("2");
        fields["Eligible Voters"].Should().Be("1");
        fields["Ineligible Members"].Should().Be("1");
    }

    // With no roll frozen the counts stay blank rather than printing zeros.
    [Test]
    public async Task FieldValues_ER08_NoRoll_LeavesCountsBlank()
    {
        (await _service.LoadFieldValuesAsync(_election.Id, "ER-08")).Should().NotContainKey("Total Members");
    }

    // ER-24 and ER-29 "Votes Cast" leave the spoiled ballots out, which have their own line.
    [TestCase("ER-24")]
    [TestCase("ER-29")]
    public async Task FieldValues_VotesCastExcludeSpoiled(string code)
    {
        _context.VoterRolls.Add(new VoterRoll { ElectionId = _election.Id, MemberId = _first.CandidateMemberId, IsEligible = true, FrozenAt = DateTime.UtcNow });
        _context.Ballots.AddRange(
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id },
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id },
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = true });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, code);

        fields["Registered Voters"].Should().Be("1");
        fields["Votes Cast"].Should().Be("2");
        fields["Spoiled Ballots"].Should().Be("1");
    }

    [Test]
    public async Task FieldValues_ER26_CountBallotsAndAuditEntries()
    {
        _context.Ballots.Add(new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-26");

        fields["Electronic Votes Recorded"].Should().Be("1");
        fields["Audit Log Entries"].Should().Be("0");
    }

    [Test]
    public async Task FieldValues_UnknownElection_ReturnsNothing()
    {
        (await _service.LoadFieldValuesAsync(9999, "ER-01")).Should().BeEmpty();
    }

    // 37.14q. ER-17 counts the ticks per nomination, names the boxes left empty and prints the
    // decision with its latest reason. A check never set counts as not ticked.
    [Test]
    public async Task ER17_PrintsTicksAndDecision()
    {
        var at = new DateTime(2026, 10, 3, 10, 0, 0, DateTimeKind.Utc);
        var user = await AddUserAsync("scrutineer");
        _context.NominationScrutinyChecks.AddRange(
            Enum.GetValues<ScrutinyCheckItem>().Where(i => i != ScrutinyCheckItem.NominationFeePaid)
                .Select(i => new NominationScrutinyCheck { NominationId = _first.Id, Item = i, IsChecked = true, SetByUserId = user, SetAt = at }));
        _context.NominationScrutinyChecks.Add(new NominationScrutinyCheck { NominationId = _second.Id, Item = ScrutinyCheckItem.VotingMember, IsChecked = false, SetByUserId = user, SetAt = at });
        _second.Status = NominationStatus.Rejected;
        _context.ScrutinyDecisions.AddRange(
            new ScrutinyDecision { NominationId = _second.Id, Accepted = false, Reason = "Old reason", DecidedByUserId = user, DecidedAt = at },
            new ScrutinyDecision { NominationId = _second.Id, Accepted = false, Reason = "Fee not paid", DecidedByUserId = user, DecidedAt = at.AddHours(1) });
        await _context.SaveChangesAsync();

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-17"))[0].Rows.ToDictionary(r => r[0]);

        rows["Alice Candidate"].Skip(2).Should().Equal("6 of 7", "Nomination fee paid", "Accepted", "");
        rows["Bob Candidate"][2].Should().Be("0 of 7");
        rows["Bob Candidate"][3].Should().StartWith("Membership verified, Voting member");
        rows["Bob Candidate"].Skip(4).Should().Equal("Rejected", "Fee not paid");
    }

    // 37.14q. ER-28 lists the request, ER-28A its decision, and ER-28B compares the recount
    // with the stored result. An undecided request prints "Not yet".
    [Test]
    public async Task ER28_Set_PrintsRequestDecisionAndDifference()
    {
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 10, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 8 });
        var received = new DateTime(2026, 12, 6, 9, 0, 0, DateTimeKind.Utc);
        var user = await AddUserAsync("clerk");
        var allowed = new ElectionRecountRequest
        {
            ElectionId = _election.Id,
            ElectionSeatId = _seat.Id,
            ApplicantName = "Bob Candidate",
            Grounds = "Close margin",
            ReceivedAt = received,
            RecordedByUserId = user,
            Allowed = true,
            DecisionReason = "Margin under 5%",
            DecidedByUserId = user,
            DecidedAt = received.AddHours(2),
            RecountedAt = received.AddDays(1)
        };
        allowed.Figures.Add(new ElectionRecountFigure { NominationId = _first.Id, VoteCount = 9 });
        allowed.Figures.Add(new ElectionRecountFigure { NominationId = _second.Id, VoteCount = 8 });
        _context.ElectionRecountRequests.AddRange(allowed, new ElectionRecountRequest
        {
            ElectionId = _election.Id,
            ElectionSeatId = _seat.Id,
            ApplicantName = "Pat Proposer",
            Grounds = "Other",
            ReceivedAt = received.AddHours(1),
            RecordedByUserId = user
        });
        await _context.SaveChangesAsync();

        var requests = (await _service.LoadTablesAsync(_election.Id, "ER-28"))[0].Rows;
        var decisions = (await _service.LoadTablesAsync(_election.Id, "ER-28A"))[0].Rows;
        var comparison = (await _service.LoadTablesAsync(_election.Id, "ER-28B")).Single();

        requests.Select(r => r[3]).Should().Equal("Bob Candidate", "Pat Proposer");
        requests[0][1].Should().Be("2026-12-06 09:00 UTC");
        requests[1][5].Should().BeEmpty();
        decisions[0].Skip(3).Should().Equal("Approved", "Margin under 5%", "2026-12-06 11:00 UTC");
        decisions[1].Skip(3).Should().Equal("Not yet", "", "Not yet");
        comparison.Heading.Should().StartWith($"R-{allowed.Id:D4}");
        comparison.Rows.Select(r => string.Join(":", r)).Should().Equal("Alice Candidate:10:9:-1", "Bob Candidate:8:8:0");
    }

    [Test]
    public async Task ER28B_NoRecount_PrintsEmptyComparison()
    {
        var tables = await _service.LoadTablesAsync(_election.Id, "ER-28B");

        tables.Single().Rows.Should().BeEmpty();
    }

    // 37.14q. ER-34 lists every handover item for the term the election fills. An item with no
    // stored tick prints as not handed over and not recorded.
    [Test]
    public async Task ER34_ListsEveryHandoverItem()
    {
        var at = new DateTime(2027, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var user = await AddUserAsync("secretary");
        _context.ECPeriodHandoverChecks.AddRange(
            new ECPeriodHandoverCheck { ECPeriodId = _election.ECPeriodId, Item = HandoverCheckItem.OfficeKeys, IsChecked = true, SetByUserId = user, SetAt = at },
            new ECPeriodHandoverCheck { ECPeriodId = _election.ECPeriodId, Item = HandoverCheckItem.ChequeBooks, IsChecked = false, SetByUserId = user, SetAt = at });
        await _context.SaveChangesAsync();

        var table = (await _service.LoadTablesAsync(_election.Id, "ER-34")).Single();
        var rows = table.Rows.ToDictionary(r => r[0]);

        table.Heading.Should().Be("Handover to 2027");
        rows.Should().HaveCount(Enum.GetValues<HandoverCheckItem>().Length);
        rows["Office keys"].Skip(1).Should().Equal("Yes", "2027-01-02 12:00 UTC");
        rows["Cheque books"].Skip(1).Should().Equal("No", "2027-01-02 12:00 UTC");
        rows["Official seal"].Skip(1).Should().Equal("No", "Not recorded");
    }

    // 37.14z. ER-25 follows the ER-04 rule, so an official who accepted stays listed after access ends.
    [Test]
    public async Task ER25_KeepsAcceptedOfficialAfterExpiry()
    {
        var official = await AddPersonaAsync("Polling Officer", Constants.Elections.PersonaGroups.Officials);
        var past = DateTime.UtcNow.AddDays(-1);
        await AddAppointmentAsync(official, "Ada Accepted", null, expiresAt: past);
        await AddAppointmentAsync(official, "Lou Lapsed", null, signed: false, expiresAt: past);
        await AddAppointmentAsync(official, "Rex Revoked", null, revoked: true);

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-25"))[1].Rows;

        rows.Should().ContainSingle();
        rows[0].Take(3).Should().Equal(Constants.Elections.PersonaGroups.Officials, "Polling Officer", "Ada Accepted");
    }

    // Owner rule 2026-10-23. Rejected ballots are the spoiled ones and total votes leave them out.
    // With one seat the winner and the best placed loser are named.
    [Test]
    public async Task FieldValues_ER27C_OneSeat_NamesWinnerAndRunnerUp()
    {
        var c = await AddMemberAsync("Cy Candidate", 5);
        var third = await AddNominationAsync(c, _first.ProposerMemberId, _first.SeconderMemberId, 3);
        _context.Ballots.AddRange(
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id },
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id },
            new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = true });
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 3 },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = third.Id, VoteCount = 3 });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-27C");
        var count = await _service.LoadFieldValuesAsync(_election.Id, "ER-27");

        fields["Spoiled Ballots"].Should().Be("1");
        fields["Total Votes"].Should().Be("2");
        fields["Winner"].Should().Be("Alice Candidate");
        fields["Runner-up"].Should().Be("Bob Candidate, Cy Candidate");
        count["Spoiled Ballots"].Should().Be("1");
        count.Should().NotContainKeys("Total Votes", "Winner");
    }

    // 37.16a. A layout seeded before the rename still says "Rejected Ballots" and must still be filled.
    [Test]
    [TestCase("ER-27")]
    [TestCase("ER-29")]
    public async Task FieldValues_OldRejectedLabel_StillFilled(string code)
    {
        _context.Ballots.Add(new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = true });
        await _context.SaveChangesAsync();

        var count = await _service.LoadFieldValuesAsync(_election.Id, code);

        count["Rejected Ballots"].Should().Be("1");
    }

    // 37.16a. The seeded layouts write "3 lines" with no label; it must print as lines, not as the words.
    [TestCase("Reasons (4 ruled lines)", "Reasons", 4)]
    [TestCase("Witnesses (1 ruled line)", "Witnesses", 1)]
    [TestCase("3 lines", "", 3)]
    [TestCase("1 line", "", 1)]
    [TestCase("40 lines", "", 10)]
    public void ParseLinesHint_ReadsLabelAndCount(string item, string label, int lines)
    {
        ElectionDocumentService.ParseLinesHint(item).Should().Be((label, lines));
    }

    [TestCase("Mark the original SPOILED and keep it.")]
    [TestCase("Lines 3 to 5 of the roll")]
    [TestCase("")]
    public void ParseLinesHint_PlainText_ReturnsNull(string item)
    {
        ElectionDocumentService.ParseLinesHint(item).Should().BeNull();
    }

    // 37.16a. Every text item in the seeded layouts is either a lines hint or a sentence, never "N lines" printed as words.
    [Test]
    public void SeededForms_NoBareLineCountPrintsAsText()
    {
        var bare = new System.Text.RegularExpressions.Regex(@"\b\d+ (ruled )?lines?\b");
        var items = _context.ElectionContents.Where(x => x.Kind == ElectionContentKind.Form).AsEnumerable()
            .SelectMany(x => System.Text.Json.JsonDocument.Parse(x.Body).RootElement.GetProperty("sections").EnumerateArray())
            .Where(s => s.GetProperty("kind").GetString() == "text")
            .SelectMany(s => s.GetProperty("items").EnumerateArray().Select(i => i.GetString()!));

        items.Where(i => bare.IsMatch(i) && ElectionDocumentService.ParseLinesHint(i) is null).Should().BeEmpty();
    }

    [Test]
    public async Task FieldValues_ER27C_SeveralSeats_PointToTheTable()
    {
        var second = new ElectionSeat { ElectionId = _election.Id, Position = EcPositionIds.VicePresident };
        _context.ElectionSeats.Add(second);
        await _context.SaveChangesAsync();
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = second.Id, NominationId = _second.Id, VoteCount = 3, IsElected = true });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-27C");

        fields["Winner"].Should().Be("See result per seat");
        fields["Runner-up"].Should().Be("See result per seat");
    }

    // The seat count comes from the election, so a second seat with no results yet still counts.
    [Test]
    public async Task FieldValues_ER27C_SecondSeatWithoutResults_PointsToTheTable()
    {
        _context.ElectionSeats.Add(new ElectionSeat { ElectionId = _election.Id, Position = EcPositionIds.VicePresident });
        _context.ElectionResults.Add(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-27C");

        fields["Winner"].Should().Be("See result per seat");
    }

    // A member archived after the count must still be named on the signed result sheet.
    [Test]
    public async Task FieldValues_ER27C_ArchivedWinner_StillNamed()
    {
        var member = (await _context.Members.FindAsync(_first.CandidateMemberId))!;
        member.IsArchived = true;
        _context.ElectionResults.Add(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true });
        await _context.SaveChangesAsync();

        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-27C");

        fields["Winner"].Should().Be("Alice Candidate");
    }

    [Test]
    public async Task FieldValues_ER27C_NoResults_LeavesWinnerBlank()
    {
        var fields = await _service.LoadFieldValuesAsync(_election.Id, "ER-27C");

        fields.Should().NotContainKeys("Winner", "Runner-up");
        fields["Total Votes"].Should().Be("0");
    }

    private async Task<int> AddUserAsync(string name)
    {
        var user = new User { Username = name, PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }

    private async Task<ElectionPersona> AddPersonaAsync(string name, string group)
    {
        var persona = new ElectionPersona { Name = name, GroupName = group, Description = "d", DeclarationText = "t", IsActive = true };
        _context.ElectionPersonas.Add(persona);
        await _context.SaveChangesAsync();
        return persona;
    }

    private async Task AddAppointmentAsync(ElectionPersona persona, string name, int? memberId, bool signed = true, bool revoked = false, DateTime? expiresAt = null)
    {
        var user = new User { Username = name.Replace(' ', '.'), PasswordHash = "x" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        _context.ElectionAppointments.Add(new ElectionAppointment
        {
            ElectionId = _election.Id,
            PersonaId = persona.Id,
            UserId = user.Id,
            MemberId = memberId,
            DisplayName = name,
            Email = name.Replace(' ', '.') + "@example.com",
            AppointedAt = now.AddDays(-2),
            AcceptedAt = signed ? now.AddDays(-1) : null,
            DeclarationSignedAt = signed ? now.AddDays(-1) : null,
            RevokedAt = revoked ? now : null,
            ExpiresAt = expiresAt
        });
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task LoadTables_FormWithoutRecords_ReturnsNothing()
    {
        (await _service.LoadTablesAsync(_election.Id, "ER-01")).Should().BeEmpty();
    }

    [Test]
    public async Task LoadTables_UnknownElection_ReturnsNothing()
    {
        (await _service.LoadTablesAsync(9999, "ER-10")).Should().BeEmpty();
    }

    // 37.14s. All 50 seeded forms print in both modes, and blank needs no election.
    [Test]
    public async Task GeneratePdf_EverySeededForm_RendersBlankAndFilled()
    {
        var codes = _context.ElectionContents.Where(x => x.Kind == ElectionContentKind.Form).Select(x => x.Key).ToList();
        codes.Should().HaveCount(50);

        foreach (var code in codes)
        {
            (await _service.GenerateBlankPdfAsync(code)).Should().NotBeNullOrEmpty(code);
            (await _service.GeneratePdfAsync(_election.Id, code, ElectionPrintMode.Blank)).Should().NotBeNullOrEmpty(code);
            (await _service.GeneratePdfAsync(_election.Id, code, ElectionPrintMode.Filled)).Should().NotBeNullOrEmpty(code);
        }
    }

    [Test]
    public async Task GeneratePdf_UnknownCode_ReturnsNull()
    {
        (await _service.GeneratePdfAsync(_election.Id, "ER-99")).Should().BeNull();
        (await _service.GenerateBlankPdfAsync("ER-99")).Should().BeNull();
        (await _service.VerifyAsync(_election.Id, "ER-99")).Should().BeNull();
    }

    [Test]
    public async Task GeneratePdf_UnknownElection_ReturnsNull()
    {
        (await _service.GeneratePdfAsync(9999, "ER-10", ElectionPrintMode.Filled)).Should().BeNull();
    }

    // An admin can unpublish a form. It stops printing, but a copy printed earlier still verifies.
    [Test]
    public async Task UnpublishedForm_DoesNotPrint_ButStillVerifies()
    {
        var row = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-07");
        row.IsPublished = false;
        await _context.SaveChangesAsync();

        (await _service.GenerateBlankPdfAsync("ER-07")).Should().BeNull();
        (await _service.GeneratePdfAsync(_election.Id, "ER-07", ElectionPrintMode.Filled)).Should().BeNull();
        (await _service.VerifyAsync(_election.Id, "er-07"))!.FormTitle.Should().Be("Confidentiality Agreement");
    }

    // The table on ER-30 gets one row per seat from the active EC positions, and no token is left.
    [Test]
    public async Task LoadForm_FillsTokensFromPositionsAndOrg()
    {
        var expected = _context.EcPositions.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToList()
            .SelectMany(p => p.SeatCount > 1 ? Enumerable.Range(1, p.SeatCount).Select(n => $"{p.Name} {n}") : [p.Name]).ToList();

        var form = await _service.LoadFormAsync("er-30");

        form!.Code.Should().Be("ER-30");
        form.Title.Should().Be("Result Declaration");
        form.Layout.Sections.Single(s => s.Kind == "table").Rows.Should().Equal(expected);
        form.Layout.Signatures.Should().NotContain(s => s.Contains("{{"));
    }

    [Test]
    public async Task GeneratePdf_NoMode_UsesTheOrgDefault()
    {
        // Only a filled copy carries the QR code, and with no logo set it is the only image.
        _config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Elections = new() { DefaultPrintMode = ElectionPrintMode.Blank } });
        PdfText(await _service.GeneratePdfAsync(_election.Id, "ER-25")).Should().NotContain("/Subtype /Image");

        _config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Elections = new() { DefaultPrintMode = ElectionPrintMode.Filled } });
        PdfText(await _service.GeneratePdfAsync(_election.Id, "ER-25")).Should().Contain("/Subtype /Image");
    }

    // The header logo comes from LogoUrl. A PNG in the web root prints; anything else is left out.
    [Test]
    public async Task GeneratePdf_LogoFromWebRoot_PrintsInTheHeader()
    {
        var root = Directory.CreateTempSubdirectory("ghcaa-logo-").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "assets"));
            await File.WriteAllBytesAsync(Path.Combine(root, "assets", "logo.png"), OnePixelPng);
            _config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto { Branding = new() { LogoUrl = "/assets/logo.png" } });
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            env.SetupGet(x => x.WebRootPath).Returns(root);
            var withLogo = new ElectionDocumentService(_context, _config.Object, environment: env.Object);
            var withoutLogo = new ElectionDocumentService(_context, _config.Object);

            var printed = await withLogo.GenerateBlankPdfAsync("ER-01");
            var plain = await withoutLogo.GenerateBlankPdfAsync("ER-01");

            PdfText(printed).Should().Contain("/Subtype /Image");
            PdfText(plain).Should().NotContain("/Subtype /Image");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void IsPngOrJpeg_ChecksTheFileSignature()
    {
        ElectionDocumentService.IsPngOrJpeg(OnePixelPng).Should().BeTrue();
        ElectionDocumentService.IsPngOrJpeg([0xFF, 0xD8, 0xFF, 0xE0, 0x00]).Should().BeTrue();
        ElectionDocumentService.IsPngOrJpeg("<svg></svg>"u8.ToArray()).Should().BeFalse();
        ElectionDocumentService.IsPngOrJpeg([]).Should().BeFalse();
    }

    private static readonly byte[] OnePixelPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

    [Test]
    public async Task Verify_KnownForm_NamesTheElectionAndForm()
    {
        var result = await _service.VerifyAsync(_election.Id, " er-27c ");

        result.Should().NotBeNull();
        result!.Reference.Should().Be($"E-{_election.Id:D6}");
        result.ElectionTitle.Should().Be(_election.Title);
        result.FormCode.Should().Be("ER-27C");
        result.FormTitle.Should().Be("Final Result Sheet");
    }

    [Test]
    public async Task Verify_UnknownElection_ReturnsNull()
    {
        (await _service.VerifyAsync(9999, "ER-10")).Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task Verify_TestElection_ReturnsNull()
    {
        _election.IsTest = true;
        await _context.SaveChangesAsync();

        (await _service.VerifyAsync(_election.Id, "ER-27C")).Should().BeNull();
    }

    // Printed QR codes hold this path. The web route verify/election/:id/:code has to keep matching it.
    [Test]
    [Category("FR-25")]
    public void VerificationPath_MatchesTheWebRoute()
    {
        ElectionDocumentService.VerificationPath(12, "ER-27C").Should().Be("/verify/election/12/ER-27C");
    }

    // 37.14o. With the glyph check on, QuestPDF throws on a letter no listed font has.
    // The font name in the file shows the Bengali letters came from Noto Sans Bengali.
    [Test]
    public async Task GeneratePdf_BengaliName_PrintsWithTheNotoFallback()
    {
        var candidate = await _context.Members.FindAsync(_first.CandidateMemberId);
        candidate!.FullName = "আলিস প্রার্থী";
        await _context.SaveChangesAsync();

        var pdf = await WithGlyphCheckAsync(() => _service.GeneratePdfAsync(_election.Id, "ER-10", ElectionPrintMode.Filled));

        PdfText(pdf).Should().Contain("NotoSansBengali");
    }

    [Test]
    public async Task BallotReceipt_BengaliTitle_RendersWithTheNotoFallback()
    {
        _election.Title = "কার্যনির্বাহী কমিটি নির্বাচন";
        await _context.SaveChangesAsync();

        var pdf = await WithGlyphCheckAsync(() => _service.GenerateBallotReceiptPdfAsync(_election.Id, "ABCD-EFGH-JKMN", DateTime.UtcNow));

        PdfText(pdf).Should().StartWith("%PDF").And.Contain("NotoSansBengali");
    }

    [Test]
    public async Task BallotReceipt_UnknownElection_ReturnsNull()
    {
        (await _service.GenerateBallotReceiptPdfAsync(9999, "ABCD-EFGH-JKMN", DateTime.UtcNow)).Should().BeNull();
    }

    [Test]
    public void AssociationTime_FormatsReceiptTimestampInBangladeshTime()
    {
        AssociationTime.FormatReceiptTimestamp(new DateTime(2026, 10, 21, 18, 30, 0, DateTimeKind.Utc))
            .Should().Be("22-10-2026 00:30");
    }

    private static async Task<byte[]?> WithGlyphCheckAsync(Func<Task<byte[]?>> generate)
    {
        var before = QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;
        try { return await generate(); }
        finally { QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = before; }
    }

    // A row edited by hand into broken JSON is treated as missing, not a 500.
    [Test]
    public async Task LoadForm_LayoutThatDoesNotParse_ReturnsNull()
    {
        var row = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-07");
        row.Body = "{\"sections\": [";
        await _context.SaveChangesAsync();

        (await _service.LoadFormAsync("ER-07")).Should().BeNull();
        (await _service.GenerateBlankPdfAsync("ER-07")).Should().BeNull();
    }

    private static string PdfText(byte[]? pdf) => System.Text.Encoding.Latin1.GetString(pdf!);
}
