using System.Text.Json;
using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// 7.18, DEPLOY-CORS-001 and 002.
[TestFixture]
public sealed class AllowedOriginsAuditTests : TestBase
{
    [TestCase("https://haragangian.com")]
    [TestCase("https://preprod.haragangian.com")]
    [TestCase("https://localhost:4200")]
    public void Problems_BareHttpsOrigin_IsAccepted(string origin) =>
        AllowedOriginsAudit.Problems([origin]).Should().BeEmpty();

    [TestCase("*")]
    [TestCase("http://haragangian.com")]
    [TestCase("haragangian.com")]
    [TestCase("https://haragangian.com/")]
    [TestCase("https://haragangian.com/api")]
    [TestCase("https://haragangian.com?x=1")]
    [TestCase("https://haragangian.com#top")]
    [TestCase("https://user:pw@haragangian.com")]
    public void Problems_AnythingElse_IsRefused(string origin) =>
        AllowedOriginsAudit.Problems(["https://haragangian.com", origin]).Should().ContainSingle();

    [Test]
    public void ClientUrl_InTheList_IsAccepted() =>
        AllowedOriginsAudit.ClientUrlProblem("https://Haragangian.com", ["https://haragangian.com"]).Should().BeNull();

    [TestCase(null)]
    [TestCase("")]
    [TestCase("https://www.haragangian.com")]
    [TestCase("https://haragangian.com/")]
    public void ClientUrl_MissingOrNotInTheList_IsRefused(string? clientUrl) =>
        AllowedOriginsAudit.ClientUrlProblem(clientUrl, ["https://haragangian.com"]).Should().NotBeNull();

    private Task<bool> RecordAsync(params string[] origins) =>
        AllowedOriginsAudit.RecordIfChangedAsync(_context, NewActivity(), NewFreeze(), origins);

    private Task<List<ActivityLog>> RowsAsync() =>
        _context.ActivityLogs.AsNoTracking()
            .Where(a => a.ActivityType == Constants.Deploy.AllowedOriginsChangedAuditType)
            .OrderBy(a => a.Id).ToListAsync();

    [Test]
    public async Task Record_FirstStart_WritesOneRow_ThenNothingWhileUnchanged()
    {
        (await RecordAsync("https://a.example")).Should().BeTrue();
        (await RecordAsync("https://a.example")).Should().BeFalse();

        var rows = await RowsAsync();
        rows.Should().ContainSingle();
        rows[0].Source.Should().Be(Constants.ActivitySources.System);
        rows[0].ActorId.Should().BeNull();
    }

    [Test]
    public async Task Record_ListChanged_KeepsOldAndNewLists()
    {
        await RecordAsync("https://a.example");

        (await RecordAsync("https://b.example", "https://a.example")).Should().BeTrue();

        using var meta = JsonDocument.Parse((await RowsAsync())[1].Metadata!);
        meta.RootElement.GetProperty("Origins").EnumerateArray().Select(e => e.GetString()).Should().Equal("https://b.example", "https://a.example");
        meta.RootElement.GetProperty("Previous").EnumerateArray().Select(e => e.GetString()).Should().Equal("https://a.example");
        meta.RootElement.GetProperty("FrozenElectionId").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Test]
    [Category("FR-39")]
    public async Task Record_ChangeDuringFreeze_IsFlagged()
    {
        await RecordAsync("https://a.example");
        var period = new ECPeriod { Title = "2027", StartDate = DateTime.UtcNow.AddDays(-1) };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();
        var election = new Election { Title = "Polling now", ECPeriodId = period.Id, Phase = ElectionPhase.Polling, NominationOpensOn = DateTime.UtcNow, NominationClosesOn = DateTime.UtcNow.AddDays(1), PollingOpensOn = DateTime.UtcNow.AddDays(2), PollingClosesOn = DateTime.UtcNow.AddDays(3) };
        _context.Elections.Add(election);
        await _context.SaveChangesAsync();

        await RecordAsync("https://b.example");

        var row = (await RowsAsync())[1];
        row.Description.Should().Contain("Polling now");
        using var meta = JsonDocument.Parse(row.Metadata!);
        meta.RootElement.GetProperty("FrozenElectionId").GetInt32().Should().Be(election.Id);
    }
}
