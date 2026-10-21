using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// Spec 023 FR-025 (37.1q): election forms fill themselves from stored records.
[TestFixture]
[Category("FR-25")]
public sealed class ElectionDocumentServiceTests : TestBase
{
    private ElectionDocumentService _service = null!;
    private Election _election = null!;
    private ElectionSeat _seat = null!;
    private Nomination _first = null!;
    private Nomination _second = null!;

    [SetUp]
    public async Task SetUpFixture()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var config = new Mock<IOrgConfigService>();
        config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        _service = new ElectionDocumentService(_context, config.Object);

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

        tables.Single().Rows.Select(r => $"{r[0]}:{r[2]}").Should().Equal("Alice Candidate:2026-10-02 14:05", "Bob Candidate:Not recorded");
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

    [Test]
    public async Task GeneratePdf_EverySupportedCode_RendersWithoutError()
    {
        foreach (var code in new[] { "ER-01", "ER-02", "ER-08", "ER-10", "ER-11", "ER-17", "ER-18", "ER-19", "ER-25", "ER-27", "ER-27C", "ER-28", "ER-28B", "ER-29", "ER-30", "ER-34" })
        {
            var pdf = await _service.GeneratePdfAsync(_election.Id, code);
            pdf.Should().NotBeNullOrEmpty(code);
        }
    }

    [Test]
    public async Task GeneratePdf_UnsupportedCode_ReturnsNull()
    {
        (await _service.GeneratePdfAsync(_election.Id, "ER-99")).Should().BeNull();
    }

    // 37.14n. The old platform codes were dropped with no aliases, so these no longer print.
    [TestCase("ER-07")]
    [TestCase("ER-09")]
    [TestCase("ER-14")]
    [TestCase("ER-23")]
    [TestCase("ER-31")]
    [TestCase("ER-33")]
    public async Task GeneratePdf_OldCodeOutsideTheNewSet_ReturnsNull(string code)
    {
        (await _service.GeneratePdfAsync(_election.Id, code)).Should().BeNull();
        (await _service.VerifyAsync(_election.Id, code)).Should().BeNull();
    }

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

        var pdf = await WithGlyphCheckAsync(() => _service.GeneratePdfAsync(_election.Id, "ER-10"));

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
    public void AssociationTime_IsSixHoursAheadOfUtc()
    {
        AssociationTime.FromUtc(new DateTime(2026, 10, 21, 18, 30, 0, DateTimeKind.Utc))
            .Should().Be(new DateTime(2026, 10, 22, 0, 30, 0));
    }

    private static async Task<byte[]?> WithGlyphCheckAsync(Func<Task<byte[]?>> generate)
    {
        var before = QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable;
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;
        try { return await generate(); }
        finally { QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = before; }
    }

    private static string PdfText(byte[]? pdf) => System.Text.Encoding.Latin1.GetString(pdf!);
}
