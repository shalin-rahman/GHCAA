using System;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public class ThemeServiceTests : TestBase
{
    private ThemeService _service = null!;
    private IMemoryCache _cache = null!;

    [SetUp]
    public void Setup()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new ThemeService(_context, _cache);
    }

    [TearDown]
    public void CacheTearDown()
    {
        _cache.Dispose();
    }

    [Category("FR-47")]
    [Test]
    public async Task CreateThemeAsync_ShouldSaveAllFields()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(7);
        var theme = new SpecialDayTheme
        {
            Title = "Victory Day",
            StartDate = start,
            EndDate = end,
            BackgroundColor = "#ff0000",
            TextColor = "#00ff00",
            IsEnabled = true
        };

        var result = await _service.CreateThemeAsync(theme);

        result.Id.Should().BeGreaterThan(0);
        var saved = await _context.SpecialDayThemes.FindAsync(result.Id);
        saved!.Title.Should().Be("Victory Day");
        saved.StartDate.Should().Be(start);
        saved.EndDate.Should().Be(end);
        saved.BackgroundColor.Should().Be("#ff0000");
        saved.TextColor.Should().Be("#00ff00");
        saved.IsEnabled.Should().BeTrue();
    }

    [Category("FR-47")]
    [Test]
    public async Task UpdateThemeAsync_ShouldUpdateAllFields()
    {
        var existing = new SpecialDayTheme
        {
            Title = "Old Theme",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(3),
            BackgroundColor = "#000000",
            TextColor = "#ffffff",
            IsEnabled = false
        };
        _context.SpecialDayThemes.Add(existing);
        await _context.SaveChangesAsync();
        DetachAll();

        var newStart = DateTime.UtcNow.Date.AddDays(10);
        var newEnd = newStart.AddDays(5);
        var update = new SpecialDayTheme
        {
            Id = existing.Id,
            Title = "New Theme",
            StartDate = newStart,
            EndDate = newEnd,
            BackgroundColor = "#111111",
            TextColor = "#eeeeee",
            IsEnabled = true
        };

        await _service.UpdateThemeAsync(update);

        var saved = await _context.SpecialDayThemes.FindAsync(existing.Id);
        saved!.Title.Should().Be("New Theme");
        saved.StartDate.Should().Be(newStart);
        saved.EndDate.Should().Be(newEnd);
        saved.BackgroundColor.Should().Be("#111111");
        saved.TextColor.Should().Be("#eeeeee");
        saved.IsEnabled.Should().BeTrue();
    }

    // 30.30: GetActiveThemeAsync must honour the theme's date window server-side (not just the
    // web admin badge). Themes outside the window (expired or not yet started) must never be
    // reported as active; only one whose window covers "now" comes back.
    [TestCase(-10, -3, false)]  // expired: EndDate in the past
    [TestCase(3, 10, false)]    // future: StartDate not yet reached
    [TestCase(-1, 1, true)]     // in window: covers "now"
    public async Task GetActiveThemeAsync_HonoursDateWindow(int startOffsetDays, int endOffsetDays, bool expectActive)
    {
        _context.SpecialDayThemes.Add(new SpecialDayTheme
        {
            Title = "Theme",
            StartDate = DateTime.UtcNow.AddDays(startOffsetDays),
            EndDate = DateTime.UtcNow.AddDays(endOffsetDays),
            BackgroundColor = "#000000",
            TextColor = "#ffffff",
            IsEnabled = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveThemeAsync();

        if (expectActive)
        {
            result.Should().NotBeNull();
            result!.Title.Should().Be("Theme");
        }
        else
        {
            result.Should().BeNull();
        }
    }
}
