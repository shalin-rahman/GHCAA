using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NewsServiceTests : TestBase
{
    private NewsService _service = null!;
    private int _authorId;

    [SetUp]
    public async Task Setup()
    {
        _service = new NewsService(_context);

        // Clear seed data so count assertions are deterministic
        _context.NewsPosts.RemoveRange(_context.NewsPosts);
        
        // Ensure an author exists
        var user = new User { Username = "author", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _authorId = user.Id;
    }

    [Test]
    public async Task GetActiveNewsAsync_ShouldOnlyReturnActivePosts()
    {
        _context.NewsPosts.AddRange(
            new NewsPost { Title = "Active 1", Content = "C1", IsActive = true, AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved },
            new NewsPost { Title = "Inactive", Content = "C3", IsActive = false, AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved }
        );
        await _context.SaveChangesAsync();

        var results = await _service.GetActiveNewsAsync();
        results.Should().HaveCount(1);
    }

    [Test]
    public async Task CreateNewsAsync_ShouldSavePostWithImageUrl()
    {
        // Act
        var dto = new CreateNewsDto { Title = "New News", Content = "Some content", ArticleCategory = Enums.ArticleCategory.Regular, ImageUrl = "/uploads/news/test.jpg" };
        var result = await _service.CreateNewsAsync(dto, _authorId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New News");
        result.ImageUrl.Should().Be("/uploads/news/test.jpg");
        var saved = await _context.NewsPosts.FindAsync(result.Id);
        saved!.ImageUrl.Should().Be("/uploads/news/test.jpg");
    }

    [Test]
    public async Task UpdateNewsAsync_ShouldModifyExistingPost()
    {
        // Arrange
        var post = new NewsPost { Title = "Old", Content = "Old C", AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateNewsAsync(new UpdateNewsDto { Id = post.Id, Title = "New Title", Content = "Updated content" });

        // Assert
        result.Title.Should().Be("New Title");
        var updated = await _context.NewsPosts.FindAsync(post.Id);
        updated!.Title.Should().Be("New Title");
    }

    [Test]
    public async Task ApproveArticleAsync_ShouldUpdateStatus()
    {
        // Arrange
        var post = new NewsPost { Title = "Pending", Content = "C", AuthorId = _authorId, Status = Enums.SubmissionStatus.Pending };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        await _service.ApproveArticleAsync(post.Id);

        // Assert
        var updated = await _context.NewsPosts.FindAsync(post.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Approved);
    }

    [Test]
    public async Task DeleteNewsAsync_ShouldRemovePost()
    {
        // Arrange
        var post = new NewsPost { Title = "To Delete", Content = "C", AuthorId = _authorId };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteNewsAsync(post.Id);

        // Assert
        result.Should().BeTrue();
        _context.NewsPosts.Any(n => n.Id == post.Id).Should().BeFalse();
    }
}

