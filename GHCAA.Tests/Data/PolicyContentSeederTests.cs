using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Data.Migrations.PgSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GHCAA.Tests.Data;

// Spec 029 FR-7, TODO 100.6: privacy, terms and FAQ pages ship as site content an admin can edit.
[TestFixture]
[Category("FR-7")]
public sealed class PolicyContentSeederTests : TestBase
{
    private static readonly string[] PolicyKeys =
        { Constants.PolicyPages.Privacy, Constants.PolicyPages.Terms, Constants.PolicyPages.Faq };

    [Test]
    public void MigrationRows_UseTheSharedKeys_AndCarryText()
    {
        SeedPolicySiteContent.Rows.Select(r => r.Key).Should().BeEquivalentTo(PolicyKeys);
        SeedPolicySiteContent.Rows.Should().OnlyContain(r => r.Body.Contains("<h2>") && r.Title.Length > 0);
    }

    [Test]
    public async Task Seed_EmptyTable_AddsThreeActivePolicyRows()
    {
        await _context.SiteContents.Where(s => s.Group == Constants.PolicyPages.Group).ExecuteDeleteAsync();

        await PolicyContentSeeder.SeedAsync(_context, NullLogger.Instance);

        var rows = await _context.SiteContents.Where(s => s.Group == Constants.PolicyPages.Group).ToListAsync();
        rows.Select(r => r.Key).Should().BeEquivalentTo(PolicyKeys);
        rows.Should().OnlyContain(r => r.IsActive && r.UpdatedByAdminId == null);
    }

    [Test]
    public async Task Seed_GroupHasAdminRows_AddsNothing_SoEditsRenamesAndDeletesStay()
    {
        await _context.SiteContents.Where(s => s.Group == Constants.PolicyPages.Group).ExecuteDeleteAsync();
        _context.SiteContents.Add(new SiteContent
        {
            Key = "faq-old",
            Group = Constants.PolicyPages.Group,
            Title = "Edited by admin",
            BodyHtml = "<p>Our own wording.</p>",
            IsActive = false,
            UpdatedByAdminId = 1,
        });
        await _context.SaveChangesAsync();

        await PolicyContentSeeder.SeedAsync(_context, NullLogger.Instance);

        var rows = await _context.SiteContents.Where(s => s.Group == Constants.PolicyPages.Group).ToListAsync();
        var kept = rows.Should().ContainSingle().Subject;
        kept.Key.Should().Be("faq-old");
        kept.BodyHtml.Should().Be("<p>Our own wording.</p>");
        kept.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task Seed_RunTwice_AddsEachKeyOnce()
    {
        await _context.SiteContents.Where(s => s.Group == Constants.PolicyPages.Group).ExecuteDeleteAsync();

        await PolicyContentSeeder.SeedAsync(_context, NullLogger.Instance);
        await PolicyContentSeeder.SeedAsync(_context, NullLogger.Instance);

        (await _context.SiteContents.CountAsync(s => s.Group == Constants.PolicyPages.Group)).Should().Be(3);
    }
}
