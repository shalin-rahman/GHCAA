using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
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
    }
}
