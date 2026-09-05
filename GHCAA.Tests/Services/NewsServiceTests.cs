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

    [Category("FR-26")]
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

    [Category("FR-26")]
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

    [Category("FR-28")]
    [Test]
    public async Task UpdateNewsAsync_ShouldModifyExistingPost()
    {
        // Arrange
        var post = new NewsPost { Title = "Old", Content = "Old C", AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        var publishDate = DateTime.UtcNow.AddDays(-2);
        var dto = new UpdateNewsDto
        {
            Id = post.Id,
            Title = "New Title",
            Content = "Updated content",
            ArticleCategory = Enums.ArticleCategory.Magazine,
            Status = Enums.SubmissionStatus.Approved,
            PostType = Enums.PostType.Notice,
            PublishDate = publishDate,
            ImageUrl = "/uploads/news/new.jpg",
            AttachmentUrl = "/uploads/news/new.pdf",
            AttachmentFileName = "new.pdf",
            IsActive = false,
            Collaborators = new List<string> { "collab@example.com" }
        };

        // Act
        var result = await _service.UpdateNewsAsync(dto);

        // Assert
        result.Title.Should().Be("New Title");
        result.Content.Should().Be("Updated content");
        var updated = await _context.NewsPosts.FindAsync(post.Id);
        updated!.Title.Should().Be("New Title");
        updated.Content.Should().Be("Updated content");
        updated.ArticleCategory.Should().Be(Enums.ArticleCategory.Magazine);
        updated.PostType.Should().Be(Enums.PostType.Notice);
        updated.PublishDate.Should().Be(publishDate);
        updated.ImageUrl.Should().Be("/uploads/news/new.jpg");
        updated.AttachmentUrl.Should().Be("/uploads/news/new.pdf");
        updated.AttachmentFileName.Should().Be("new.pdf");
        updated.IsActive.Should().BeFalse();
        updated.ExternalCollaborators.Should().Be("collab@example.com");
    }

    // 57.1 audit flag: `existing.Status = dto.Status` runs unconditionally, unlike PublishDate two
    // lines above it (guarded by `dto.PublishDate.HasValue`). UpdateNewsDto inherits Status from
    // CreateNewsDto, which defaults to Approved — so a caller building an update DTO for an
    // unrelated field change, without setting Status, would silently flip a Pending post to
    // Approved. This proves the DTO default reaches the DB: it is a real bug, not a missing
    // assertion, since UpdateNewsAsync gives no way to say "leave status alone."
    [Test]
    public async Task UpdateNewsAsync_OverwritesStatus_WithDtoDefault_WhenCallerDoesNotSetIt()
    {
        var post = new NewsPost { Title = "Pending Post", Content = "C", AuthorId = _authorId, Status = Enums.SubmissionStatus.Pending };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        // Deliberately does not set Status - relies on UpdateNewsDto's inherited default.
        var dto = new UpdateNewsDto { Id = post.Id, Title = "Pending Post", Content = "C" };
        dto.Status.Should().Be(Enums.SubmissionStatus.Approved, "this is the DTO default the audit flagged, not a value this test chose");

        await _service.UpdateNewsAsync(dto);

        var updated = await _context.NewsPosts.FindAsync(post.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Approved,
            "UpdateNewsAsync assigns existing.Status = dto.Status unconditionally, so an update that never intended to touch status silently approves a Pending post");
    }

    [Category("FR-27")]
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

    [Category("FR-28")]
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

