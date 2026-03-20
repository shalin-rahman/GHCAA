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
        var user = new User { Username = "newsauthor", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.NewsPosts.AddRange(
            new NewsPost { Title = "Active 1", Content = "C1", IsActive = true, AuthorId = user.Id, ArticleCategory = Enums.ArticleCategory.Regular, Status = Enums.SubmissionStatus.Approved },
            new NewsPost { Title = "Active 2", Content = "C2", IsActive = true, AuthorId = user.Id, ArticleCategory = Enums.ArticleCategory.Event, Status = Enums.SubmissionStatus.Approved },
            new NewsPost { Title = "Inactive", Content = "C3", IsActive = false, AuthorId = user.Id, ArticleCategory = Enums.ArticleCategory.Regular, Status = Enums.SubmissionStatus.Approved }
        );
        await _context.SaveChangesAsync();

        var results = await _service.GetActiveNewsAsync();

        results.Should().HaveCount(2);
        results.All(n => n.IsActive).Should().BeTrue();
    }
}
