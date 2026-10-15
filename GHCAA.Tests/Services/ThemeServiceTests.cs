using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
        var theme = new SpecialDayThemeSaveDto
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
        var update = new SpecialDayThemeSaveDto
        {
            Title = "New Theme",
            StartDate = newStart,
            EndDate = newEnd,
            BackgroundColor = "#111111",
            TextColor = "#eeeeee",
            IsEnabled = true
        };

        var updated = await _service.UpdateThemeAsync(existing.Id, update);

        updated.Should().BeTrue();

        var saved = await _context.SpecialDayThemes.FindAsync(existing.Id);
        saved!.Title.Should().Be("New Theme");
        saved.StartDate.Should().Be(newStart);
        saved.EndDate.Should().Be(newEnd);
        saved.BackgroundColor.Should().Be("#111111");
        saved.TextColor.Should().Be("#eeeeee");
        saved.IsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task UpdateThemeAsync_ReturnsFalse_WhenThemeMissing()
    {
        var updated = await _service.UpdateThemeAsync(999, new SpecialDayThemeSaveDto { Title = "X", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow });

        updated.Should().BeFalse();
    }

    // 84.30: create ignores a posted Id so a client cannot pick the primary key.
    [Test]
    public async Task CreateThemeAsync_IgnoresPostedId()
    {
        var result = await _service.CreateThemeAsync(new SpecialDayThemeSaveDto { Id = 777, Title = "X", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow });

        result.Id.Should().NotBe(777);
    }

    // 62.56g: Title + StartDate is the theme's key (IX_SpecialDayThemes_Title_StartDate).
    private static SpecialDayThemeSaveDto KeyedTheme(string title, DateTime start) => new()
    {
        Title = title,
        StartDate = start,
        EndDate = start.AddDays(1),
        BackgroundColor = "#000000",
        TextColor = "#ffffff"
    };

    [Test]
    [Category("FR-47")]
    public async Task CreateThemeAsync_Throws_WhenTitleAndStartDateTaken()
    {
        var start = new DateTime(2026, 2, 21, 0, 0, 0, DateTimeKind.Utc);
        await _service.CreateThemeAsync(KeyedTheme("Language Day", start));

        var act = () => _service.CreateThemeAsync(KeyedTheme("Language Day", start));

        await act.Should().ThrowAsync<DuplicateKeyException>().WithMessage("*Language Day*2026-02-21*");
        (await _context.SpecialDayThemes.CountAsync(t => t.Title == "Language Day")).Should().Be(1);
    }

    [Test]
    [Category("FR-47")]
    public async Task CreateThemeAsync_AllowsSameTitle_OnAnotherStartDate()
    {
        await _service.CreateThemeAsync(KeyedTheme("Language Day", new DateTime(2026, 2, 21, 0, 0, 0, DateTimeKind.Utc)));

        await _service.CreateThemeAsync(KeyedTheme("Language Day", new DateTime(2027, 2, 21, 0, 0, 0, DateTimeKind.Utc)));

        (await _context.SpecialDayThemes.CountAsync(t => t.Title == "Language Day")).Should().Be(2);
    }

    [Test]
    [Category("FR-47")]
    public async Task UpdateThemeAsync_Throws_WhenMovedOntoAnotherThemesKey()
    {
        var start = new DateTime(2026, 2, 21, 0, 0, 0, DateTimeKind.Utc);
        await _service.CreateThemeAsync(KeyedTheme("Language Day", start));
        var other = await _service.CreateThemeAsync(KeyedTheme("Victory Day", start.AddMonths(10)));

        var edit = KeyedTheme("Language Day", start);
        edit.Id = other.Id;
        var act = () => _service.UpdateThemeAsync(other.Id, edit);

        await act.Should().ThrowAsync<DuplicateKeyException>();
    }

    [Test]
    [Category("FR-47")]
    public async Task UpdateThemeAsync_KeepingItsOwnKey_Succeeds()
    {
        var start = new DateTime(2026, 2, 21, 0, 0, 0, DateTimeKind.Utc);
        var created = await _service.CreateThemeAsync(KeyedTheme("Language Day", start));

        var edit = KeyedTheme("Language Day", start);
        edit.Id = created.Id;
        edit.TextColor = "#eeeeee";

        (await _service.UpdateThemeAsync(created.Id, edit)).Should().BeTrue();
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
