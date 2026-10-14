using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    internal static class PagingExtensions
    {
        // The query must already be ordered on a unique key, or rows can repeat or go missing between pages.
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, Constants.Paging.MaxPageSize);

            var totalItems = await query.CountAsync(cancellationToken);
            // Checked in long so a huge page number returns an empty page instead of overflowing Skip.
            var skip = (long)(page - 1) * pageSize;
            var items = skip >= totalItems
                ? new List<T>()
                : await query
                    .Skip((int)skip)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

            return new PagedResult<T>
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
