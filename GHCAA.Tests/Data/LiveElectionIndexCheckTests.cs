using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Data;

// 37.14e: the startup warning names the elections that stop IX_Elections_ECPeriodId_Live from
// being built. The test database has the index from the model, so each test drops it first,
// the way a database the migration skipped looks.
[TestFixture]
[Category("FR-42")]
public sealed class LiveElectionIndexCheckTests : TestBase
{
    private int _period;
    private int _otherPeriod;

    [SetUp]
    public async Task SetUpFixture()
    {
        await _context.Database.ExecuteSqlRawAsync($"DROP INDEX IF EXISTS \"{Constants.Elections.LiveIndexName}\"");
        var periods = new[] { new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow }, new ECPeriod { Title = "2029", StartDate = DateTime.UtcNow } };
        _context.ECPeriods.AddRange(periods);
        await _context.SaveChangesAsync();
        (_period, _otherPeriod) = (periods[0].Id, periods[1].Id);
    }

    private async Task<Election> AddElectionAsync(int periodId, string title, ElectionPhase phase = ElectionPhase.Announced, bool isTest = false)
    {
        var election = new Election { Title = title, ECPeriodId = periodId, Phase = phase, IsTest = isTest, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow, PollingOpensOn = DateTime.UtcNow, PollingClosesOn = DateTime.UtcNow };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();
        return election;
    }

    [Test]
    public async Task NoClash_ReturnsNothing()
    {
        await AddElectionAsync(_period, "A");
        await AddElectionAsync(_otherPeriod, "B");

        (await MigrationBootstrapper.LiveElectionsSharingAPeriodAsync(_context)).Should().BeEmpty();
    }

    [Test]
    public async Task TwoLiveElectionsInOnePeriod_AreBothListed()
    {
        var a = await AddElectionAsync(_period, "A");
        var b = await AddElectionAsync(_period, "B");
        await AddElectionAsync(_otherPeriod, "C");

        var clashes = await MigrationBootstrapper.LiveElectionsSharingAPeriodAsync(_context);

        clashes.Should().Equal((a.Id, "A", _period), (b.Id, "B", _period));
    }

    [Test]
    public async Task ArchivedAndTestElections_DoNotCount()
    {
        await AddElectionAsync(_period, "Live");
        await AddElectionAsync(_period, "Old", ElectionPhase.Archived);
        await AddElectionAsync(_period, "Rehearsal", isTest: true);

        (await MigrationBootstrapper.LiveElectionsSharingAPeriodAsync(_context)).Should().BeEmpty();
    }
}
