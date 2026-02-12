using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext _db;

        public NewsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<NewsPost>> GetActiveNewsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<NewsPost>> GetAllNewsForAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<NewsPost?> GetNewsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.NewsPosts
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<NewsPost> CreateNewsAsync(NewsPost post, CancellationToken cancellationToken = default)
        {
            post.PublishDate = DateTime.UtcNow;
            await _db.NewsPosts.AddAsync(post, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return post;
        }

        public async Task<NewsPost> UpdateNewsAsync(NewsPost post, CancellationToken cancellationToken = default)
        {
            var existing = await _db.NewsPosts.FindAsync(new object[] { post.Id }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Post not found");

            existing.Title = post.Title;
            existing.Content = post.Content;
            existing.Category = post.Category;
            existing.IsActive = post.IsActive;
            existing.LastModified = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteNewsAsync(int id, CancellationToken cancellationToken = default)
        {
            var post = await _db.NewsPosts.FindAsync(new object[] { id }, cancellationToken);
            if (post == null) return false;

            _db.NewsPosts.Remove(post);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
