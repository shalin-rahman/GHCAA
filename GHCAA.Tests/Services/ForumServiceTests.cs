using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class ForumServiceTests : TestBase
    {
        private ForumService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new ForumService(_context);
        }

        [Test]
        public async Task GetCategoriesAsync_ReturnsSeededCategory()
        {
            _context.ForumCategories.Add(new ForumCategory
            {
                Name = "General",
                Description = "Test category",
                SortOrder = 1,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var categories = await _service.GetCategoriesAsync();

            categories.Should().HaveCount(1);
            categories[0].Name.Should().Be("General");
        }

        [Test]
        public async Task CreateTopicAsync_AndCreatePostAsync_ShouldPersist()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var category = new ForumCategory { Name = "Alumni", SortOrder = 1, IsActive = true };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();

            var topic = await _service.CreateTopicAsync(new CreateForumTopicDto
            {
                CategoryId = category.Id,
                Title = "Smoke topic",
                Content = "First post body"
            }, member.Id);

            topic.Id.Should().BeGreaterThan(0);
            topic.Title.Should().Be("Smoke topic");

            var post = await _service.CreatePostAsync(new CreateForumPostDto
            {
                TopicId = topic.Id,
                Content = "Reply content"
            }, member.Id);

            post.Id.Should().BeGreaterThan(0);
            post.Content.Should().Be("Reply content");
        }

        // ForumTopicConfiguration.HasQueryFilter hides a topic once its category is deactivated
        // (IsActive && Category.IsActive). DeleteTopicAsync used to look the topic up via
        // FindAsync, which honors that filter, so an admin could never reach (and soft-delete) a
        // topic orphaned under an already-deactivated category. It now uses
        // IgnoreQueryFilters().FirstOrDefaultAsync so the row is still reachable.
        [Category("FR-44")]
        [Test]
        public async Task DeleteTopicAsync_ShouldSoftDelete_TopicHiddenByQueryFilter_ViaIgnoreQueryFilters()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var category = new ForumCategory { Name = "Retired Category", SortOrder = 1, IsActive = false };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();

            var topic = new ForumTopic
            {
                CategoryId = category.Id,
                Title = "Orphaned Topic",
                Content = "Body",
                AuthorId = member.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();
            DetachAll();

            // Sanity check: the default query filter really does hide this topic.
            (await _context.ForumTopics.FirstOrDefaultAsync(t => t.Id == topic.Id)).Should().BeNull();

            await _service.DeleteTopicAsync(topic.Id, member.Id, isSuperAdmin: true);

            var reloaded = await _context.ForumTopics.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == topic.Id);
            reloaded.Should().NotBeNull();
            reloaded!.IsActive.Should().BeFalse();
        }

        [Test]
        public async Task DeleteTopicAsync_ShouldThrow_WhenNotAuthorAndNotSuperAdmin()
        {
            var author = await CreateAndSaveTestMemberAsync(name: "Author", email: "author@test.com", phone: "01711111111", nid: "1111111111");
            var category = new ForumCategory { Name = "Active", SortOrder = 1, IsActive = true };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();

            var topic = new ForumTopic { CategoryId = category.Id, Title = "T", Content = "C", AuthorId = author.Id, CreatedAt = DateTime.UtcNow, IsActive = true };
            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            Func<Task> act = async () => await _service.DeleteTopicAsync(topic.Id, memberId: 999, isSuperAdmin: false);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // ForumPostConfiguration.HasQueryFilter hides a post once its author is archived
        // (Author == null || !Author.IsArchived). DeletePostAsync now uses
        // IgnoreQueryFilters().FirstOrDefaultAsync so a superadmin can still moderate a post whose
        // author has since been archived.
        [Category("FR-44")]
        [Test]
        public async Task DeletePostAsync_ShouldSoftDelete_PostHiddenByQueryFilter_ViaIgnoreQueryFilters()
        {
            var author = await CreateAndSaveTestMemberAsync();
            var category = new ForumCategory { Name = "Active", SortOrder = 1, IsActive = true };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();

            var topic = new ForumTopic { CategoryId = category.Id, Title = "T", Content = "C", AuthorId = author.Id, CreatedAt = DateTime.UtcNow, IsActive = true };
            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            var post = new ForumPost { TopicId = topic.Id, Content = "Reply", AuthorId = author.Id, CreatedAt = DateTime.UtcNow, IsActive = true };
            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            author.IsArchived = true;
            await _context.SaveChangesAsync();
            DetachAll();

            // Sanity check: the default query filter really does hide this post now.
            (await _context.ForumPosts.FirstOrDefaultAsync(p => p.Id == post.Id)).Should().BeNull();

            await _service.DeletePostAsync(post.Id, author.Id, isSuperAdmin: true);

            var reloaded = await _context.ForumPosts.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == post.Id);
            reloaded.Should().NotBeNull();
            reloaded!.IsActive.Should().BeFalse();
        }

        // These used to be bare Exceptions, which ExceptionMiddleware turned into a 500.
        [Category("FR-44")]
        [Test]
        public async Task CreateTopicAsync_ThrowsInvalidOperation_WhenCategoryInactive()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var category = new ForumCategory { Name = "Closed", SortOrder = 1, IsActive = false };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();

            Func<Task> act = () => _service.CreateTopicAsync(new CreateForumTopicDto { CategoryId = category.Id, Title = "T", Content = "C" }, member.Id);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Category("FR-44")]
        [Test]
        public async Task CreatePostAsync_ThrowsKeyNotFound_WhenTopicMissing()
        {
            var member = await CreateAndSaveTestMemberAsync();

            Func<Task> act = () => _service.CreatePostAsync(new CreateForumPostDto { TopicId = 999, Content = "C" }, member.Id);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Category("FR-44")]
        [Test]
        public async Task CreatePostAsync_ThrowsInvalidOperation_WhenTopicLocked()
        {
            var member = await CreateAndSaveTestMemberAsync();
            var category = new ForumCategory { Name = "Open", SortOrder = 1, IsActive = true };
            _context.ForumCategories.Add(category);
            await _context.SaveChangesAsync();
            var topic = new ForumTopic { CategoryId = category.Id, Title = "T", Content = "C", AuthorId = member.Id, CreatedAt = DateTime.UtcNow, IsActive = true, IsLocked = true };
            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            Func<Task> act = () => _service.CreatePostAsync(new CreateForumPostDto { TopicId = topic.Id, Content = "C" }, member.Id);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Category("FR-44")]
        [TestCase(0, "Title", "Body")]
        [TestCase(1, "", "Body")]
        [TestCase(1, "Title", "")]
        public void CreateForumTopicDto_FailsValidation_OnMissingInput(int categoryId, string title, string content)
        {
            var dto = new CreateForumTopicDto { CategoryId = categoryId, Title = title, Content = content };

            Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true).Should().BeFalse();
        }

        [Category("FR-44")]
        [Test]
        public void ForumDtos_FailValidation_WhenOverLength()
        {
            var topic = new CreateForumTopicDto { CategoryId = 1, Title = new string('x', 201), Content = "Body" };
            var post = new CreateForumPostDto { TopicId = 1, Content = new string('x', 10001) };

            Validator.TryValidateObject(topic, new ValidationContext(topic), new List<ValidationResult>(), true).Should().BeFalse();
            Validator.TryValidateObject(post, new ValidationContext(post), new List<ValidationResult>(), true).Should().BeFalse();
        }
    }
}
