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

namespace GHCAA.Infrastructure.Services
{
    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext _db;

        public NewsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<NewsPostDto>> GetActiveNewsAsync(Enums.ArticleCategory? category = null, CancellationToken cancellationToken = default)
        {
            var query = _db.NewsPosts
                .Where(n => n.IsActive && n.Status == Enums.SubmissionStatus.Approved);

            if (category.HasValue)
            {
                query = query.Where(n => n.ArticleCategory == category.Value);
            }

            return await query
                .OrderByDescending(n => n.PublishDate)
                .Select(n => MapToDto(n))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NewsPostDto>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .OrderByDescending(n => n.PublishDate)
                .Select(n => MapToDto(n))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NewsPostDto>> GetPendingSubmissionsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .Where(n => n.Status == Enums.SubmissionStatus.Pending)
                .OrderByDescending(n => n.PublishDate)
                .Select(n => MapToDto(n))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NewsPostDto>> GetMySubmissionsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .Where(n => n.AuthorId == userId)
                .OrderByDescending(n => n.PublishDate)
                .Select(n => MapToDto(n))
                .ToListAsync(cancellationToken);
        }

        public async Task<NewsPostDto?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts
                .IgnoreQueryFilters()
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
            
            return post == null ? null : MapToDto(post);
        }

        public async Task<NewsPostDto> CreateNewsAsync(CreateNewsDto dto, int authorId, CancellationToken cancellationToken = default)
        {
            var post = new NewsPost
            {
                Title = dto.Title,
                Content = dto.Content,
                ArticleCategory = dto.ArticleCategory,
                Status = dto.Status,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                AuthorId = authorId,
                PublishDate = DateTime.UtcNow
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
            existing.Content = dto.Content;
            existing.ArticleCategory = dto.ArticleCategory;
            existing.Status = dto.Status;
            existing.ImageUrl = dto.ImageUrl;
            existing.IsActive = dto.IsActive;
            existing.LastModified = DateTime.UtcNow;

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

        private static NewsPostDto MapToDto(NewsPost post)
        {
            return new NewsPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ArticleCategory = post.ArticleCategory,
                Status = post.Status,
                ImageUrl = post.ImageUrl,
                IsActive = post.IsActive,
                CreatedAt = post.PublishDate,
                AuthorName = post.Author?.Member?.FullName ?? post.Author?.Username ?? "Unknown"
            };
        }
    }
}
