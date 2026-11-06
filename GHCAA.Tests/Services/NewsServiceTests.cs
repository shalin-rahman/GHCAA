using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NewsServiceTests : TestBase
{
    private NewsService _service = null!;
    private int _authorId;

    [SetUp]
    public async Task Setup()
    {
        _service = NewService(new OrgConfigDto());

        // Clear seed data so count assertions are deterministic
        _context.NewsPosts.RemoveRange(_context.NewsPosts);

        // Ensure an author exists
        var user = new User { Username = "author", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _authorId = user.Id;
    }

    private NewsService NewService(OrgConfigDto config)
    {
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(config);
        return new NewsService(_context, new MemoryCache(new MemoryCacheOptions()), orgConfig.Object);
    }

    [Category("FR-26")]
    [Test]
    public async Task TryCountImageUploadAsync_StopsAtDailyLimit_PerAccount()
    {
        var service = NewService(new OrgConfigDto { Workflow = new WorkflowDto { NewsImageUploadsPerDay = 2 } });

        (await service.TryCountImageUploadAsync(1)).Should().BeTrue();
        (await service.TryCountImageUploadAsync(1)).Should().BeTrue();
        (await service.TryCountImageUploadAsync(1)).Should().BeFalse();
        (await service.TryCountImageUploadAsync(2)).Should().BeTrue();
    }

    [Category("FR-26")]
    [Test]
    public async Task ReleaseImageUploadAsync_GivesBackASlot_AndNeverGoesBelowZero()
    {
        var service = NewService(new OrgConfigDto { Workflow = new WorkflowDto { NewsImageUploadsPerDay = 1 } });

        await service.ReleaseImageUploadAsync(1);
        (await service.TryCountImageUploadAsync(1)).Should().BeTrue();
        await service.ReleaseImageUploadAsync(1);
        await service.ReleaseImageUploadAsync(1);

        (await service.TryCountImageUploadAsync(1)).Should().BeTrue();
        (await service.TryCountImageUploadAsync(1)).Should().BeFalse();
    }

    [Category("FR-26")]
    [Test]
    public async Task TryCountImageUploadAsync_LimitBelowOne_MeansNoLimit()
    {
        var service = NewService(new OrgConfigDto { Workflow = new WorkflowDto { NewsImageUploadsPerDay = 0 } });

        for (var i = 0; i < 50; i++)
            (await service.TryCountImageUploadAsync(1)).Should().BeTrue();
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

    [Category("FR-27")]
    [Test]
    public async Task GetActiveNewsAsync_LeavesOutMembersOnlyPosts_UnlessAsked()
    {
        _context.NewsPosts.AddRange(
            new NewsPost { Title = "Public", Content = "C1", IsActive = true, AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved },
            new NewsPost { Title = "Members", Content = "C2", IsActive = true, AuthorId = _authorId, Status = Enums.SubmissionStatus.Approved, MembersOnly = true }
        );
        await _context.SaveChangesAsync();

        (await _service.GetActiveNewsAsync()).Select(n => n.Title).Should().Equal("Public");
        (await _service.GetActiveNewsAsync(includeMembersOnly: true)).Select(n => n.Title).Should().BeEquivalentTo("Public", "Members");
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

    // PollService posts its announcement through this method with a relative link to the poll page.
    // The sanitizer must keep that href, or the announcement shows a dead link.
    [Category("FR-26")]
    [Test]
    public async Task CreateNewsAsync_KeepsRelativePollLink()
    {
        var link = $"<a href=\"{Constants.Discussion.PollPagePath}\">Open the poll</a>";
        var dto = new CreateNewsDto { Title = "Poll open", Content = $"<p>Vote now.</p><p>{link}</p>", ArticleCategory = Enums.ArticleCategory.Regular };

        var result = await _service.CreateNewsAsync(dto, _authorId);

        result.Content.Should().Contain($"href=\"{Constants.Discussion.PollPagePath}\"");
        result.Content.Should().Contain("Open the poll");
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

    // 57.4: fixed. UpdateNewsAsync used to assign existing.Status = dto.Status unconditionally, so
    // a caller that never touched Status still got UpdateNewsDto's inherited default (Approved),
    // silently flipping a Pending post to Approved. Status changes now only happen through
    // ApproveArticleAsync/RejectArticleAsync.
    [Category("FR-28")]
    [Test]
    public async Task UpdateNewsAsync_DoesNotChangeStatus_EvenWhenDtoCarriesTheDefault()
    {
        var post = new NewsPost { Title = "Pending Post", Content = "C", AuthorId = _authorId, Status = Enums.SubmissionStatus.Pending };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        // Deliberately does not set Status - relies on UpdateNewsDto's inherited default.
        var dto = new UpdateNewsDto { Id = post.Id, Title = "Pending Post", Content = "C" };
        dto.Status.Should().Be(Enums.SubmissionStatus.Approved, "this is the DTO default the fix must not let leak into an unrelated edit");

        await _service.UpdateNewsAsync(dto);

        var updated = await _context.NewsPosts.FindAsync(post.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Pending,
            "an edit that never intended to touch status must not move a Pending post to Approved");
    }

    [Category("FR-28")]
    [Test]
    public async Task UpdateNewsAsync_WithoutMembersOnly_KeepsThePostMembersOnly()
    {
        var post = new NewsPost { Title = "Poll news", Content = "C", AuthorId = _authorId, MembersOnly = true };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();

        await _service.UpdateNewsAsync(new UpdateNewsDto { Id = post.Id, Title = "Poll news", Content = "C" });

        (await _context.NewsPosts.FindAsync(post.Id))!.MembersOnly.Should().BeTrue();
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

    private async Task<NewsPost> AddPostAsync(Enums.SubmissionStatus status, bool isActive)
    {
        var post = new NewsPost { Title = "Post", Content = "C", AuthorId = _authorId, Status = status, IsActive = isActive };
        _context.NewsPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    // 84.86: anonymous callers used to read drafts and pending posts by Id.
    [Category("FR-27")]
    [TestCase(Enums.SubmissionStatus.Pending, false)]
    [TestCase(Enums.SubmissionStatus.Draft, false)]
    [TestCase(Enums.SubmissionStatus.Approved, false)]
    [TestCase(Enums.SubmissionStatus.Rejected, true)]
    public async Task GetNewsByIdAsync_UnpublishedPost_IsHiddenFromOtherViewers(Enums.SubmissionStatus status, bool isActive)
    {
        var post = await AddPostAsync(status, isActive);

        (await _service.GetNewsByIdAsync(post.Id, null, false)).Should().BeNull();
        (await _service.GetNewsByIdAsync(post.Id, _authorId + 1000, false)).Should().BeNull();
    }

    [Category("FR-27")]
    [Test]
    public async Task GetNewsByIdAsync_PublishedPost_IsReadableByAnyone()
    {
        var post = await AddPostAsync(Enums.SubmissionStatus.Approved, true);

        (await _service.GetNewsByIdAsync(post.Id, null, false)).Should().NotBeNull();
    }

    [Category("FR-27")]
    [Test]
    public async Task GetNewsByIdAsync_UnpublishedPost_OpensForAdminAuthorAndCollaborator()
    {
        var post = await AddPostAsync(Enums.SubmissionStatus.Pending, false);
        var collaborator = new User { Username = "collab", PasswordHash = "hash" };
        _context.Users.Add(collaborator);
        await _context.SaveChangesAsync();
        _context.NewsCollaborators.Add(new NewsCollaborator { NewsPostId = post.Id, UserId = collaborator.Id });
        await _context.SaveChangesAsync();

        (await _service.GetNewsByIdAsync(post.Id, null, true)).Should().NotBeNull();
        (await _service.GetNewsByIdAsync(post.Id, _authorId, false)).Should().NotBeNull();
        (await _service.GetNewsByIdAsync(post.Id, collaborator.Id, false)).Should().NotBeNull();
    }
}

