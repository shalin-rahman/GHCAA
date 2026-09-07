using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ErrorLogService> _logger;

        // 45.6: no dedicated cleanup job — rows older than RetentionDays are swept out on roughly
        // 1 in 20 writes instead. An error-log table only ever grows on error traffic, which is
        // bursty and low-volume compared to normal request traffic, so a cheap sampling check here
        // avoids running a DELETE on every single insert without needing a new hosted-service.
        private static readonly Random _sampler = new();
        private const int CleanupSampleOneInN = 20;

        public ErrorLogService(ApplicationDbContext db, ILogger<ErrorLogService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogAsync(string level, string message, string? exceptionType, string? stackTrace, string? source,
            string? requestPath, string? requestMethod, int? userId, string? username,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entry = new ErrorLog
                {
                    OccurredAt = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    ExceptionType = exceptionType,
                    StackTrace = stackTrace,
                    Source = source,
                    RequestPath = requestPath,
                    RequestMethod = requestMethod,
                    UserId = userId,
                    Username = username
                };

                await _db.ErrorLogs.AddAsync(entry, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                if (_sampler.Next(CleanupSampleOneInN) == 0)
                {
                    await CleanupOldRowsAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Logging must never throw back into ExceptionMiddleware — that would replace the
                // real error it was trying to record with a DB failure instead.
                _logger.LogWarning(ex, "Failed to persist ErrorLog entry (message: {Message})", message);
            }
        }

        private async Task CleanupOldRowsAsync(CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(-Constants.ErrorLogs.RetentionDays);
            await _db.ErrorLogs
                .Where(e => e.OccurredAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<PagedResult<ErrorLogDto>> GetPagedAsync(ErrorLogFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.ErrorLogs.AsQueryable();

            if (filter.FromDate.HasValue)
            {
                query = query.Where(e => e.OccurredAt >= filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                query = query.Where(e => e.OccurredAt <= filter.ToDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(filter.Level) && !filter.Level.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.Level == filter.Level);
            }
            if (!string.IsNullOrWhiteSpace(filter.Query))
            {
                var term = filter.Query.Trim().ToLower();
                query = query.Where(e =>
                    e.Message.ToLower().Contains(term) ||
                    (e.ExceptionType != null && e.ExceptionType.ToLower().Contains(term)) ||
                    (e.StackTrace != null && e.StackTrace.ToLower().Contains(term)));
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, Constants.ErrorLogs.MaxPageSize);
            var page = filter.Page <= 0 ? 1 : filter.Page;

            var items = await query
                .OrderByDescending(e => e.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ErrorLogDto
                {
                    Id = e.Id,
                    OccurredAt = e.OccurredAt,
                    Level = e.Level,
                    Message = e.Message,
                    ExceptionType = e.ExceptionType,
                    StackTrace = e.StackTrace,
                    Source = e.Source,
                    RequestPath = e.RequestPath,
                    RequestMethod = e.RequestMethod,
                    UserId = e.UserId,
                    Username = e.Username
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ErrorLogDto>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
