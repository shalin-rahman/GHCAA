using System.Text.Json.Nodes;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Data;

// Spec 023 FR-025, TODO 37.14s: election pages and form layouts load from the profile and an
// admin's edit survives the next boot.
[TestFixture]
[Category("FR-25")]
public sealed class ElectionContentSeederTests : TestBase
{
    [Test]
    public void Load_DefaultProfile_HasEightPagesAndFiftyForms()
    {
        var rows = ElectionContentSeeder.Load();

        rows.Count(r => r.Kind == ElectionContentKind.Page).Should().Be(8);
        rows.Count(r => r.Kind == ElectionContentKind.Form).Should().Be(50);
        rows.Select(r => (r.Kind, r.Key.ToUpperInvariant())).Should().OnlyHaveUniqueItems();
        rows.Should().Contain(r => r.Key == "regulations" && r.Format == ElectionPageFormat.Prose);
        rows.Should().Contain(r => r.Key == "er-25" && r.Format == ElectionPageFormat.Form);
    }

    [Test]
    public void Load_FormLayout_LeavesOutCodeAndTitle()
    {
        var form = ElectionContentSeeder.Load().Single(r => r.Key == "ER-01");

        form.Title.Should().Be("Election Notice");
        var layout = JsonNode.Parse(form.Body)!.AsObject();
        layout.ContainsKey("code").Should().BeFalse();
        layout.ContainsKey("title").Should().BeFalse();
        layout["sections"]!.AsArray().Should().NotBeEmpty();
    }

    [Test]
    public async Task Sync_AddsMissingKeys_AndKeepsAnAdminEdit()
    {
        _context.ElectionContents.Add(new ElectionContent
        {
            Key = "regulations",
            Kind = ElectionContentKind.Page,
            Format = ElectionPageFormat.Prose,
            Title = "Edited by admin",
            Body = "Edited body",
            LastModified = DateTime.UtcNow,
            UpdatedByAdminId = 1,
        });
        await _context.SaveChangesAsync();

        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);

        (await _context.ElectionContents.CountAsync()).Should().Be(58);
        var kept = await _context.ElectionContents.SingleAsync(x => x.Key == "regulations");
        kept.Title.Should().Be("Edited by admin");
        kept.Body.Should().Be("Edited body");
    }

    // 37.16b. A row seeded before a wording change takes the new text, since no admin saved it.
    [Test]
    public async Task Sync_RefreshesRowNoAdminSaved_KeepsPublishFlag()
    {
        _context.ElectionContents.Add(new ElectionContent
        {
            Key = "ER-29",
            Kind = ElectionContentKind.Form,
            Title = "Old title",
            Body = """{"header":["Rejected Ballots"],"sections":[{"name":"","kind":"text","items":["x"]}]}""",
            IsPublished = false,
        });
        await _context.SaveChangesAsync();

        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);

        var row = await _context.ElectionContents.SingleAsync(x => x.Key == "ER-29");
        var seeded = ElectionContentSeeder.Load().Single(r => r.Key == "ER-29");
        row.Title.Should().Be(seeded.Title);
        row.Body.Should().Be(seeded.Body).And.Contain("Spoiled Ballots");
        row.IsPublished.Should().BeFalse();
        row.UpdatedByAdminId.Should().BeNull();
    }

    [Test]
    public async Task Sync_SecondRun_AddsNothing()
    {
        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);
        DetachAll();
        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);

        (await _context.ElectionContents.CountAsync()).Should().Be(58);
    }

    // docs/Elections holds the same pages for readers of the repository. The two copies drift
    // unless something checks them.
    [Test]
    public void ProfilePages_MatchDocsElections()
    {
        var pagesJson = ElectionContentSeeder.PagesManifestPath()!;
        var root = new DirectoryInfo(Path.GetDirectoryName(pagesJson)!);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "docs", "Elections"))) root = root.Parent;
        root.Should().NotBeNull();

        foreach (var page in JsonNode.Parse(File.ReadAllText(pagesJson))!.AsArray())
        {
            var file = (string)page!["file"]!;
            var profileCopy = File.ReadAllText(Path.Combine(Path.GetDirectoryName(pagesJson)!, "pages", file));
            var docsCopy = File.ReadAllText(Path.Combine(root!.FullName, "docs", "Elections", file));
            profileCopy.Should().Be(docsCopy, $"profiles/default/election-content/pages/{file} must match docs/Elections/{file}");
        }
    }
}
