using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;

namespace GHCAA.Tests.Services;

[TestFixture]
public class SiteContentServiceTests : TestBase
{
    private SiteContentService _service = null!;

    [SetUp]
    public async Task Setup()
    {
        _service = new SiteContentService(_context);
        _context.SiteContents.RemoveRange(_context.SiteContents);
        await _context.SaveChangesAsync();
    }

    [Category("FR-45")]
        [Test]
    public async Task GetActiveByGroupAsync_FiltersByGroupAndActive_OrderedByDisplayOrder()
    {
        _context.SiteContents.AddRange(
            new SiteContent { Key = "a2", Group = "about", Title = "Second", BodyHtml = "<p>2</p>", DisplayOrder = 2 },
            new SiteContent { Key = "a1", Group = "about", Title = "First", BodyHtml = "<p>1</p>", DisplayOrder = 1 },
            new SiteContent { Key = "a3", Group = "about", Title = "Hidden", BodyHtml = "<p>3</p>", DisplayOrder = 3, IsActive = false },
            new SiteContent { Key = "c1", Group = "contact", Title = "Contact", BodyHtml = "<p>c</p>", DisplayOrder = 1 }
        );
        await _context.SaveChangesAsync();

        var results = (await _service.GetActiveByGroupAsync("about")).ToList();

        results.Should().HaveCount(2);
        results[0].Key.Should().Be("a1");
        results[1].Key.Should().Be("a2");
    }

    [Category("FR-45")]
        [Test]
    public async Task CreateAsync_SanitizesBodyHtml()
    {
        var dto = new UpsertSiteContentDto
        {
            Key = "about-origin",
            Group = "about",
            Title = "Origin",
            BodyHtml = "<p>Safe</p><script>alert('x')</script>"
        };

        var result = await _service.CreateAsync(dto, adminId: 1);

        result.BodyHtml.Should().NotContain("<script");
        result.BodyHtml.Should().Contain("Safe");
    }

    [Category("FR-45")]
        [Category("FR-43")]
        [Test]
    public async Task UpdateAsync_PersistsChangesAndStampsAdmin()
    {
        var created = await _service.CreateAsync(
            new UpsertSiteContentDto { Key = "k", Group = "about", Title = "Old", BodyHtml = "<p>old</p>" }, adminId: 1);

        var updated = await _service.UpdateAsync(created.Id,
            new UpsertSiteContentDto { Key = "k", Group = "about", Title = "New", BodyHtml = "<p>new</p>", DisplayOrder = 4 }, adminId: 2);

        updated.Title.Should().Be("New");
        updated.DisplayOrder.Should().Be(4);

        var saved = await _context.SiteContents.FindAsync(created.Id);
        saved!.UpdatedByAdminId.Should().Be(2);
        saved.LastModified.Should().NotBeNull();
    }

    [Category("FR-45")]
        [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenMissing()
    {
        (await _service.DeleteAsync(9999)).Should().BeFalse();
    }
}
