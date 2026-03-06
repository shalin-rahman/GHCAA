using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NewsServiceTests : TestBase
{
    private NewsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new NewsService(_context);

        // Clear seed data so count assertions are deterministic
        _context.NewsPosts.RemoveRange(_context.NewsPosts);
        _context.SaveChanges();
    }

    [Test]
    public async Task GetActiveNewsAsync_ShouldOnlyReturnActivePosts()
    {
        _context.NewsPosts.AddRange(
            new NewsPost { Title = "Active 1", Content = "C1", IsActive = true, AuthorId = 1, Category = Enums.NewsCategory.News },
            new NewsPost { Title = "Active 2", Content = "C2", IsActive = true, AuthorId = 1, Category = Enums.NewsCategory.OrganisationalUpdate },
            new NewsPost { Title = "Inactive", Content = "C3", IsActive = false, AuthorId = 1, Category = Enums.NewsCategory.BusinessInformation }
        );
        await _context.SaveChangesAsync();

        var results = await _service.GetActiveNewsAsync();

        results.Should().HaveCount(2);
        results.All(n => n.IsActive).Should().BeTrue();
    }
}
