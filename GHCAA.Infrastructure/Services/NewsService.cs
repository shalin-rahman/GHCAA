using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GHCAA.Domain;
using Ganss.Xss;

namespace GHCAA.Infrastructure.Services
{
    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext _db;
        // 24.42: Shared, stateless sanitizer instance — HtmlSanitizer is thread-safe.
        private static readonly HtmlSanitizer _sanitizer = new();

        public NewsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<NewsPostDto>> GetActiveNewsAsync(Enums.ArticleCategory? category = null, Enums.PostType? postType = null, CancellationToken cancellationToken = default)
        {
            var query = _db.NewsPosts
                .Where(n => n.IsActive && n.Status == Enums.SubmissionStatus.Approved);

            if (category.HasValue)
            {
                query = query.Where(n => n.ArticleCategory == category.Value);
            }

            if (postType.HasValue)
            {
                query = query.Where(n => n.PostType == postType.Value);
            }

            var posts = await query
                .Include(n => n.Author)
                    .ThenInclude(u => u!.Member)
                .OrderByDescending(n => n.PublishDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<NewsPostDto>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default)
        {
            var posts = await _db.NewsPosts
                .Include(n => n.Author)
                    .ThenInclude(u => u!.Member)
                .OrderByDescending(n => n.PublishDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<NewsPostDto>> GetPendingSubmissionsAsync(CancellationToken cancellationToken = default)
        {
            var posts = await _db.NewsPosts
                .Include(n => n.Author)
                    .ThenInclude(u => u!.Member)
                .Where(n => n.Status == Enums.SubmissionStatus.Pending)
                .OrderByDescending(n => n.PublishDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<NewsPostDto>> GetMySubmissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var posts = await _db.NewsPosts
                .Include(n => n.Author)
                    .ThenInclude(u => u!.Member)
                .Where(n => n.AuthorId == userId)
                .OrderByDescending(n => n.PublishDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return posts.Select(MapToDto);
        }

        public async Task<NewsPostDto?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts
                .IgnoreQueryFilters()
                .Include(n => n.Author)
                .Include(n => n.Collaborators)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u!.Member)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

            return post == null ? null : MapToDto(post);
        }

        public async Task<NewsPostDto> CreateNewsAsync(CreateNewsDto dto, int authorId, CancellationToken cancellationToken = default)
        {
            var post = new NewsPost
            {
                Title = dto.Title,
                Content = _sanitizer.Sanitize(dto.Content ?? ""), // 24.42: strip XSS before storage
                ArticleCategory = dto.ArticleCategory,
                Status = dto.Status,
                PostType = dto.PostType,
                ImageUrl = dto.ImageUrl,
                AttachmentUrl = dto.AttachmentUrl,
                AttachmentFileName = dto.AttachmentFileName,
                IsActive = dto.IsActive,
                AuthorId = authorId,
                PublishDate = DateTime.UtcNow,
                ExternalCollaborators = dto.Collaborators != null ? string.Join(", ", dto.Collaborators) : null
            };

            await _db.NewsPosts.AddAsync(post, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(post);
        }

        public async Task<NewsPostDto> UpdateNewsAsync(UpdateNewsDto dto, CancellationToken cancellationToken = default)
        {
            var existing = await _db.NewsPosts.FindAsync(new object[] { dto.Id }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Post not found");

            existing.Title = dto.Title;
            existing.Content = _sanitizer.Sanitize(dto.Content ?? ""); // 24.42
            existing.ArticleCategory = dto.ArticleCategory;
            existing.Status = dto.Status;
            existing.PostType = dto.PostType;
            existing.ImageUrl = dto.ImageUrl;
            existing.AttachmentUrl = dto.AttachmentUrl;
            existing.AttachmentFileName = dto.AttachmentFileName;
            existing.IsActive = dto.IsActive;
            existing.LastModified = DateTime.UtcNow;
            existing.ExternalCollaborators = dto.Collaborators != null ? string.Join(", ", dto.Collaborators) : null;

            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(existing);
        }

        public async Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts.FindAsync(new object[] { id }, cancellationToken);
            if (post == null) return false;

            _db.NewsPosts.Remove(post);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ApproveArticleAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts.FindAsync(new object[] { id }, cancellationToken);
            if (post == null) return false;

            post.Status = Enums.SubmissionStatus.Approved;
            post.IsActive = true;
            post.PublishDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RejectArticleAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts.FindAsync(new object[] { id }, cancellationToken);
            if (post == null) return false;

            post.Status = Enums.SubmissionStatus.Rejected;
            post.IsActive = false;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AddCollaboratorAsync(int newsPostId, int userId, CancellationToken cancellationToken = default)
        {
            var exists = await _db.NewsCollaborators.AnyAsync(nc => nc.NewsPostId == newsPostId && nc.UserId == userId, cancellationToken);
            if (exists) return true;

            var collab = new NewsCollaborator
            {
                NewsPostId = newsPostId,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };

            await _db.NewsCollaborators.AddAsync(collab, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemoveCollaboratorAsync(int newsPostId, int userId, CancellationToken cancellationToken = default)
        {
            var collab = await _db.NewsCollaborators.FirstOrDefaultAsync(nc => nc.NewsPostId == newsPostId && nc.UserId == userId, cancellationToken);
            if (collab == null) return false;

            _db.NewsCollaborators.Remove(collab);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static NewsPostDto MapToDto(NewsPost post)
        {
            return new NewsPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ArticleCategory = post.ArticleCategory,
                Status = post.Status,
                PostType = post.PostType,
                ImageUrl = post.ImageUrl,
                AttachmentUrl = post.AttachmentUrl,
                AttachmentFileName = post.AttachmentFileName,
                IsActive = post.IsActive,
                CreatedAt = post.PublishDate,
                AuthorName = post.Author?.Member?.FullName ?? post.Author?.Username ?? "Unknown",
                Collaborators = post.ExternalCollaborators?.Split(", ").ToList() ?? new List<string>()
            };
        }
    }
}
