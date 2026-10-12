using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Moq;
using NUnit.Framework;
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
        _seat = new ElectionSeat { Position = ECPosition.President };
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
    public async Task ER23_CarriesPerCandidateCounts()
    {
        // Three ballots: one spoiled, one for Alice, one that left the seat empty.
        var cast = Enumerable.Range(0, 3).Select(i => new Ballot { Id = Guid.NewGuid(), ElectionId = _election.Id, IsSpoiled = i == 2 }).ToList();
        _context.Ballots.AddRange(cast);
        _context.BallotVotes.Add(new BallotVote { Id = Guid.NewGuid(), BallotId = cast[0].Id, ElectionSeatId = _seat.Id, NominationId = _first.Id });
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 1, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 0 });
        await _context.SaveChangesAsync();

        var tables = await _service.LoadTablesAsync(_election.Id, "ER-23");

        tables[0].Rows.Single().Should().Equal("3", "1", "2");
        tables[1].Rows.Select(r => string.Join(":", r)).Should().Equal("Alice Candidate:1", "Bob Candidate:0", "Abstain:1");
    }

    [Test]
    public async Task ER10_ListsFilingTimes()
    {
        var tables = await _service.LoadTablesAsync(_election.Id, "er-10");

        tables.Single().Rows.Select(r => r[0]).Should().Equal("Alice Candidate", "Bob Candidate");
        tables.Single().Rows.Should().OnlyContain(r => r[2] == "2026-10-01 09:30");
    }

    [Test]
    public async Task ER09_ListsProposerAndSeconder()
    {
        var row = (await _service.LoadTablesAsync(_election.Id, "ER-09")).Single().Rows[0];

        row.Should().Contain(new[] { "Alice Candidate", "Pat Proposer", "Sam Seconder" });
    }

    [Test]
    public async Task ER13_ListsOnlyWithdrawnNominations()
    {
        _second.Status = NominationStatus.Withdrawn;
        _second.WithdrawnAt = new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc);
        await _context.SaveChangesAsync();

        (await _service.LoadTablesAsync(_election.Id, "ER-13")).Single().Rows.Select(r => r[0]).Should().Equal("Bob Candidate");
    }

    [Test]
    public async Task ER29_ShowsOutcomePerSeat()
    {
        _context.ElectionResults.AddRange(
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _first.Id, VoteCount = 5, IsElected = true },
            new ElectionResult { ElectionId = _election.Id, ElectionSeatId = _seat.Id, NominationId = _second.Id, VoteCount = 2 });
        await _context.SaveChangesAsync();

        var rows = (await _service.LoadTablesAsync(_election.Id, "ER-29"))[1].Rows;

        rows.Select(r => r[^1]).Should().Equal("Elected", "Not elected");
    }

    [Test]
    public async Task LoadTables_FormWithoutRecords_ReturnsNothing()
    {
        (await _service.LoadTablesAsync(_election.Id, "ER-01")).Should().BeEmpty();
    }

    [Test]
    public async Task LoadTables_UnknownElection_ReturnsNothing()
    {
        (await _service.LoadTablesAsync(9999, "ER-09")).Should().BeEmpty();
    }

    [Test]
    public async Task GeneratePdf_EverySupportedCode_RendersWithoutError()
    {
        foreach (var code in new[] { "ER-01", "ER-02", "ER-07", "ER-09", "ER-10", "ER-11", "ER-13", "ER-14", "ER-19", "ER-23", "ER-26", "ER-28", "ER-29", "ER-30", "ER-31", "ER-33" })
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
}
