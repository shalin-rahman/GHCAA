using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _db;

        public LookupService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IDictionary<string, IEnumerable<LookupDto>>> GetAllLookupsAsync(CancellationToken cancellationToken = default)
        {
            var items = await _db.Lookups
                .Where(l => l.IsActive)
                .OrderBy(l => l.Category)
                .ThenBy(l => l.DisplayOrder)
                .ToListAsync(cancellationToken);

            return items.GroupBy(l => l.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(l => new LookupDto
                    {
                        Value = l.Value,
                        Label = l.Label,
                        DisplayOrder = l.DisplayOrder
                    })
                );
        }

        public async Task<IEnumerable<LookupDto>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _db.Lookups
                .Where(l => l.Category == category && l.IsActive)
                .OrderBy(l => l.DisplayOrder)
                .Select(l => new LookupDto
                {
                    Value = l.Value,
                    Label = l.Label,
                    DisplayOrder = l.DisplayOrder
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LookupItem> AddLookupItemAsync(LookupItem item, CancellationToken cancellationToken = default)
        {
            _db.Lookups.Add(item);
            await _db.SaveChangesAsync(cancellationToken);
            return item;
        }

        public async Task<bool> UpdateLookupItemAsync(int id, LookupItem item, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Lookups.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null) return false;

            existing.Label = item.Label;
            existing.Value = item.Value;
            existing.DisplayOrder = item.DisplayOrder;
            existing.IsActive = item.IsActive;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteLookupItemAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Lookups.FindAsync(new object[] { id }, cancellationToken);
            if (existing == null) return false;

            _db.Lookups.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
