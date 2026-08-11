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

    // 30.30: GetActiveThemeAsync must honour the theme's date window server-side (not just the
    // web admin badge). An expired theme (EndDate in the past) must never be reported as active.
    [Test]
    public async Task GetActiveThemeAsync_WithExpiredTheme_ShouldReturnNull()
    {
        _context.SpecialDayThemes.Add(new SpecialDayTheme
        {
            Title = "Expired Theme",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-3),
            BackgroundColor = "#000000",
            TextColor = "#ffffff",
            IsEnabled = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveThemeAsync();

        result.Should().BeNull();
    }

    [Test]
    public async Task GetActiveThemeAsync_WithFutureTheme_ShouldReturnNull()
    {
        _context.SpecialDayThemes.Add(new SpecialDayTheme
        {
            Title = "Future Theme",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(10),
            BackgroundColor = "#000000",
            TextColor = "#ffffff",
            IsEnabled = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveThemeAsync();

        result.Should().BeNull();
    }

    [Test]
    public async Task GetActiveThemeAsync_WithThemeInWindow_ShouldReturnTheme()
    {
        _context.SpecialDayThemes.Add(new SpecialDayTheme
        {
            Title = "Live Theme",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            BackgroundColor = "#000000",
            TextColor = "#ffffff",
            IsEnabled = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveThemeAsync();

        result.Should().NotBeNull();
        result!.Title.Should().Be("Live Theme");
    }
}
