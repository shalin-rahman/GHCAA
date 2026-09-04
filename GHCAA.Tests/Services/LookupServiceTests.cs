using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public class LookupServiceTests : TestBase
{
    private LookupService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new LookupService(_context);
    }

    [Test]
    public async Task UpdateLookupItemAsync_ShouldUpdateAllFields()
    {
        var existing = new LookupItem
        {
            LookupGroup = "ProfessionalSector",
            Value = "old-value",
            Label = "Old Label",
            DisplayOrder = 1,
            IsActive = true
        };
        _context.Lookups.Add(existing);
        await _context.SaveChangesAsync();

        var update = new LookupItem
        {
            LookupGroup = "ProfessionalSector",
            Value = "new-value",
            Label = "New Label",
            DisplayOrder = 9,
            IsActive = false
        };

        var result = await _service.UpdateLookupItemAsync(existing.Id, update);

        result.Should().BeTrue();
        var saved = await _context.Lookups.FindAsync(existing.Id);
        saved!.Value.Should().Be("new-value");
        saved.Label.Should().Be("New Label");
        saved.DisplayOrder.Should().Be(9);
        saved.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task UpdateLookupItemAsync_ReturnsFalse_WhenNotFound()
    {
        var result = await _service.UpdateLookupItemAsync(999_999, new LookupItem { LookupGroup = "G", Value = "v", Label = "L" });

        result.Should().BeFalse();
    }
}
