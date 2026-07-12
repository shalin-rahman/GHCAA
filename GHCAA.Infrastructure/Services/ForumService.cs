using Ganss.Xss;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Infrastructure.Services
{
    public class ForumService : IForumService
    {
        private readonly ApplicationDbContext _context;
        // 1d: Shared, stateless sanitizer instance — HtmlSanitizer is thread-safe.
        private static readonly HtmlSanitizer _sanitizer = new();

        public ForumService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ForumCategoryDto>> GetCategoriesAsync()
        {
            return await _context.ForumCategories
                .Select(c => new ForumCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    SortOrder = c.SortOrder,
                    TopicCount = c.Topics.Count(t => t.IsActive),
                    PostCount = c.Topics.SelectMany(t => t.Posts).Count(p => p.IsActive)
                })
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<ForumTopicDto>> GetTopicsByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var query = _context.ForumTopics
                .Include(t => t.Author)
                .Where(t => t.CategoryId == categoryId);

            return await query
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.LastUpdatedAt ?? t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new ForumTopicDto
                {
                    Id = t.Id,
                    CategoryId = t.CategoryId,
                    Title = t.Title,
                    Content = t.Content,
                    AuthorId = t.AuthorId,
                    AuthorName = t.Author != null ? t.Author.FullName : "Unknown",
                    AuthorPhotoUrl = t.Author != null ? t.Author.PhotoPath : null,
                    CreatedAt = t.CreatedAt,
                    LastUpdatedAt = t.LastUpdatedAt,
                    ViewCount = t.ViewCount,
                    IsPinned = t.IsPinned,
                    IsLocked = t.IsLocked,
                    ReplyCount = t.Posts.Count(p => p.IsActive)
                })
                .ToListAsync();
        }

        public async Task<ForumTopicDto?> GetTopicByIdAsync(int topicId)
        {
            var topic = await _context.ForumTopics
                .Include(t => t.Author)
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null) return null;

            // Increment view count
            topic.ViewCount++;
            await _context.SaveChangesAsync();

            return new ForumTopicDto
            {
                Id = topic.Id,
                CategoryId = topic.CategoryId,
                Title = topic.Title,
                Content = topic.Content,
                AuthorId = topic.AuthorId,
                AuthorName = topic.Author != null ? topic.Author.FullName : "Unknown",
                AuthorPhotoUrl = topic.Author != null ? topic.Author.PhotoPath : null,
                CreatedAt = topic.CreatedAt,
                LastUpdatedAt = topic.LastUpdatedAt,
                ViewCount = topic.ViewCount,
                IsPinned = topic.IsPinned,
                IsLocked = topic.IsLocked,
                ReplyCount = await _context.ForumPosts.CountAsync(p => p.TopicId == topicId && p.IsActive)
            };
        }

        public async Task<List<ForumPostDto>> GetPostsByTopicAsync(int topicId, int page, int pageSize)
        {
            return await _context.ForumPosts
                .Include(p => p.Author)
                .Where(p => p.TopicId == topicId)
                .OrderBy(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ForumPostDto
                {
                    Id = p.Id,
                    TopicId = p.TopicId,
                    Content = p.Content,
                    AuthorId = p.AuthorId,
                    AuthorName = p.Author != null ? p.Author.FullName : "Unknown",
                    AuthorPhotoUrl = p.Author != null ? p.Author.PhotoPath : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    ParentPostId = p.ParentPostId
                })
                .ToListAsync();
        }

        public async Task<ForumTopicDto> CreateTopicAsync(CreateForumTopicDto dto, int authorId)
        {
            var category = await _context.ForumCategories.FindAsync(dto.CategoryId);
            if (category == null || !category.IsActive)
                throw new Exception("Invalid or inactive category.");

            var topic = new ForumTopic
            {
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Content = _sanitizer.Sanitize(dto.Content ?? ""), // 1d: strip XSS before storage
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            return await GetTopicByIdAsync(topic.Id) ?? throw new Exception("Topic created but could not be retrieved.");
        }

        public async Task<ForumPostDto> CreatePostAsync(CreateForumPostDto dto, int authorId)
        {
            var topic = await _context.ForumTopics.FindAsync(dto.TopicId);
            if (topic == null || !topic.IsActive)
                throw new Exception("Topic not found or is inactive.");

            if (topic.IsLocked)
                throw new Exception("Topic is locked and cannot receive new posts.");

            var post = new ForumPost
            {
                TopicId = dto.TopicId,
                Content = _sanitizer.Sanitize(dto.Content ?? ""), // 1d: strip XSS before storage
                AuthorId = authorId,
                ParentPostId = dto.ParentPostId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ForumPosts.Add(post);
            
            topic.LastUpdatedAt = DateTime.UtcNow; // Bump topic

            await _context.SaveChangesAsync();

            return new ForumPostDto
            {
                Id = post.Id,
                TopicId = post.TopicId,
                Content = post.Content,
                AuthorId = post.AuthorId,
                AuthorName = "Self", // Client typically knows who they are, but a reload from DB is safer
                CreatedAt = post.CreatedAt
            };
        }

        public async Task DeleteTopicAsync(int topicId, int memberId, bool isSuperAdmin)
        {
            var topic = await _context.ForumTopics.FindAsync(topicId);
            if (topic == null) return;

            if (topic.AuthorId != memberId && !isSuperAdmin)
                throw new UnauthorizedAccessException("You can only delete your own topics.");

            topic.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int postId, int memberId, bool isSuperAdmin)
        {
            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null) return;

            if (post.AuthorId != memberId && !isSuperAdmin)
                throw new UnauthorizedAccessException("You can only delete your own posts.");

            post.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
