using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

// Spec 023 FR-048 (37.13a): the hash chain kept for each election.
[TestFixture]
[Category("FR-48")]
public sealed class ElectionAuditWriterTests : TestBase
{
    private ElectionAuditWriter _writer = null!;
    private int _electionId;

    [SetUp]
    public async Task SetUpFixture()
    {
        _writer = new ElectionAuditWriter(_context);
        _electionId = await AddElectionAsync();
    }

    private async Task<int> AddElectionAsync()
    {
        var period = new ECPeriod { Title = "P" + Guid.NewGuid(), StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var e = new Election { Title = "E", ECPeriodId = period.Id, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(e);
        await _context.SaveChangesAsync();
        return e.Id;
    }

    private async Task AppendThreeAsync()
    {
        await _writer.AppendAsync(_electionId, Constants.Elections.AuditActions.PhaseChanged, 1, new { to = "Nomination" });
        await _writer.AppendAsync(_electionId, Constants.Elections.AuditActions.Counted, 1, new { ballots = 10 });
        await _writer.AppendAsync(_electionId, Constants.Elections.AuditActions.Declared, 1, null);
        await _writer.SaveAsync();
    }

    [Test]
    public async Task Verify_EmptyChain_IsOk()
    {
        var r = await _writer.VerifyAsync(_electionId);

        r.Ok.Should().BeTrue();
        r.Entries.Should().Be(0);
        r.FirstBrokenSeq.Should().BeNull();
    }

    [Test]
    public async Task Append_LinksEntriesFromTheGenesisHash()
    {
        await AppendThreeAsync();

        var rows = await _context.ElectionAuditEntries.AsNoTracking().OrderBy(x => x.Seq).ToListAsync();
        rows.Select(x => x.Seq).Should().Equal(1, 2, 3);
        rows[0].PrevHash.Should().Be(Constants.Elections.AuditGenesisHash);
        rows[1].PrevHash.Should().Be(rows[0].Hash);
        rows[2].PrevHash.Should().Be(rows[1].Hash);
        (await _writer.VerifyAsync(_electionId)).Should().Be(new GHCAA.Application.DTOs.ElectionAuditVerifyDto(true, 3, null));
    }

    [Test]
    public async Task Append_AcrossSaves_KeepsTheChain()
    {
        await _writer.AppendAsync(_electionId, "A", null, null);
        await _writer.SaveAsync();
        await _writer.AppendAsync(_electionId, "B", null, null);
        await _writer.SaveAsync();

        (await _writer.VerifyAsync(_electionId)).Ok.Should().BeTrue();
    }

    [Test]
    public async Task Chains_AreIndependentPerElection()
    {
        var other = await AddElectionAsync();
        await _writer.AppendAsync(_electionId, "A", null, null);
        await _writer.AppendAsync(other, "A", null, null);
        await _writer.SaveAsync();

        _context.ElectionAuditEntries.AsNoTracking().Where(x => x.ElectionId == other).Single().Seq.Should().Be(1);
        (await _writer.VerifyAsync(other)).Ok.Should().BeTrue();
    }

    [Test]
    public async Task Verify_ChangedDetail_NamesTheFirstBrokenEntry()
    {
        await AppendThreeAsync();
        await _context.Database.ExecuteSqlRawAsync("UPDATE ElectionAuditEntries SET DetailJson = '{{\"ballots\":99}}' WHERE Seq = 2");

        var r = await _writer.VerifyAsync(_electionId);

        r.Ok.Should().BeFalse();
        r.FirstBrokenSeq.Should().Be(2);
    }

    [Test]
    public async Task Verify_RemovedMiddleEntry_NamesTheEntryAfterTheGap()
    {
        await AppendThreeAsync();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM ElectionAuditEntries WHERE Seq = 2");

        var r = await _writer.VerifyAsync(_electionId);

        r.Ok.Should().BeFalse();
        r.FirstBrokenSeq.Should().Be(3);
    }

    [Test]
    public async Task Verify_RemovedFirstEntry_NamesTheNewFirst()
    {
        await AppendThreeAsync();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM ElectionAuditEntries WHERE Seq = 1");

        (await _writer.VerifyAsync(_electionId)).FirstBrokenSeq.Should().Be(2);
    }

    [Test]
    [Category("NFR-R5")]
    public async Task Save_WhenAnotherWriterTookTheSeq_NumbersTheEntryAgain()
    {
        await _writer.AppendAsync(_electionId, "Mine", 1, null);
        // A second writer saved Seq 1 first. Raw SQL keeps it out of this context's tracker.
        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO ElectionAuditEntries (ElectionId, Seq, At, Action, DetailJson, PrevHash, Hash) VALUES ({0}, 1, '2026-10-10 00:00:00', 'Other', '{{}}', {1}, 'h1')",
            _electionId, Constants.Elections.AuditGenesisHash);

        await _writer.SaveAsync();

        var mine = _context.ElectionAuditEntries.AsNoTracking().Single(x => x.Action == "Mine");
        mine.Seq.Should().Be(2);
        mine.PrevHash.Should().Be("h1");
        mine.Hash.Should().Be(ElectionAuditWriter.ComputeHash("h1", _electionId, 2, mine.At, 1, "Mine", mine.DetailJson));
    }

    [Test]
    public async Task Verify_ChangedActor_NamesTheEntry()
    {
        await AppendThreeAsync();
        await _context.Database.ExecuteSqlRawAsync("UPDATE ElectionAuditEntries SET ActorUserId = 99 WHERE Seq = 2");

        (await _writer.VerifyAsync(_electionId)).FirstBrokenSeq.Should().Be(2);
    }

    [Test]
    public async Task List_ReturnsEntriesInOrderWithoutPrevHash()
    {
        await AppendThreeAsync();

        var rows = await _writer.ListAsync(_electionId);

        rows.Select(x => x.Seq).Should().Equal(1, 2, 3);
        rows.Select(x => x.Action).Should().Equal(Constants.Elections.AuditActions.PhaseChanged, Constants.Elections.AuditActions.Counted, Constants.Elections.AuditActions.Declared);
    }

    [Test]
    public async Task Detail_HoldsNoVoterIdentity()
    {
        await _writer.AppendAsync(_electionId, Constants.Elections.AuditActions.Counted, 7, new { ballots = 3 });
        await _writer.SaveAsync();

        var row = _context.ElectionAuditEntries.AsNoTracking().Single();
        row.DetailJson.Should().Be("{\"ballots\":3}");
        row.ActorUserId.Should().Be(7);
    }
}
