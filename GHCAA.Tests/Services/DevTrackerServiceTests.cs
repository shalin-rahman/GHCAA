using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

// Coverage for docs/TODO.md 82.115: the admin Developer Options screen parses docs/TODO.md
// itself, so the parser needs to hold up against the file's actual format quirks (dated
// DONE tags, items with and without a Depends-on clause, mixed statuses).
[TestFixture]
public class DevTrackerServiceTests
{
    private string _root = null!;

    [SetUp]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "ghcaa-dev-tracker-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_root);
    }

    [TearDown]
    public void TearDown()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best-effort cleanup */ }
    }

    private IHostEnvironment EnvironmentAt(string contentRoot)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(contentRoot);
        return env.Object;
    }

    private DevTrackerService CreateService(string contentRoot) =>
        new(EnvironmentAt(contentRoot), NullLogger<DevTrackerService>.Instance);

    private void WriteTodo(string contents)
    {
        Directory.CreateDirectory(Path.Combine(_root, "docs"));
        File.WriteAllText(Path.Combine(_root, "docs", "TODO.md"), contents);
    }

    [Test]
    public async Task GetOpenItemsAsync_ReturnsTodoAndPartialItems_WithWorkPackageAndPriority()
    {
        WriteTodo("""
            # Work Package 82 — Sample batch

            82.1 [TODO] **Priority: P2 | Depends on: none.** First open item.
            82.2 [DONE 2026-09-01] **Priority: P1.** Already closed, must not appear.
            82.3 [PARTIAL] **Priority: P3 | Depends on: 82.1.** Half-finished item.
            """);

        var items = await CreateService(_root).GetOpenItemsAsync(new DevTrackerFilterDto());

        items.Should().HaveCount(2);
        items.Should().Contain(i => i.Id == "82.1" && i.Status == "TODO" && i.Priority == "P2"
            && i.DependsOn == null && i.WorkPackageNumber == 82 && i.Summary == "First open item.");
        items.Should().Contain(i => i.Id == "82.3" && i.Status == "PARTIAL" && i.DependsOn == "82.1");
        items.Should().NotContain(i => i.Id == "82.2");
    }

    [Test]
    public async Task GetOpenItemsAsync_ParsesOlderItemFormat_WithNoDependsOnClause()
    {
        WriteTodo("""
            # Work Package 6 — Legacy batch

            6.2 [TODO] **Priority: P3.** Alumni referral system for jobs and internships.
            """);

        var items = await CreateService(_root).GetOpenItemsAsync(new DevTrackerFilterDto());

        items.Should().ContainSingle();
        items[0].Priority.Should().Be("P3");
        items[0].DependsOn.Should().BeNull();
    }

    [Test]
    public async Task GetOpenItemsAsync_FiltersByPriority_WhenRequested()
    {
        WriteTodo("""
            # Work Package 1 — Batch

            1.1 [TODO] **Priority: P1 | Depends on: none.** High priority item.
            1.2 [TODO] **Priority: P4 | Depends on: none.** Low priority item.
            """);

        var items = await CreateService(_root).GetOpenItemsAsync(new DevTrackerFilterDto { Priority = "P1" });

        items.Should().ContainSingle();
        items[0].Id.Should().Be("1.1");
    }

    [Test]
    public async Task GetOpenItemsAsync_TodoFileMissing_ReturnsEmptyListInsteadOfThrowing()
    {
        // No docs/TODO.md written at all — this is an internal admin screen; a missing file
        // should show an empty tracker, not break the admin area.
        var items = await CreateService(_root).GetOpenItemsAsync(new DevTrackerFilterDto());

        items.Should().BeEmpty();
    }

    [Test]
    public async Task GetOpenItemsAsync_TodoFileOneLevelAboveContentRoot_IsAlsoFound()
    {
        WriteTodo("""
            # Work Package 1 — Batch

            1.1 [TODO] **Priority: P2 | Depends on: none.** Found via parent lookup.
            """);
        var apiLikeContentRoot = Path.Combine(_root, "GHCAA.API");
        Directory.CreateDirectory(apiLikeContentRoot);

        var items = await CreateService(apiLikeContentRoot).GetOpenItemsAsync(new DevTrackerFilterDto());

        items.Should().ContainSingle();
    }
}
