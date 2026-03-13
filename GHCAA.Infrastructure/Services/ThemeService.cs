using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public interface IThemeService
    {
        Task<SpecialDayTheme?> GetActiveThemeAsync();
        Task<List<SpecialDayTheme>> GetAllThemesAsync();
        Task<SpecialDayTheme> CreateThemeAsync(SpecialDayTheme theme);
        Task UpdateThemeAsync(SpecialDayTheme theme);
        Task DeleteThemeAsync(int id);
    }

    public class ThemeService : IThemeService
    {
        private readonly ApplicationDbContext _context;

        public ThemeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SpecialDayTheme?> GetActiveThemeAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.SpecialDayThemes
                .Where(t => t.IsEnabled && t.StartDate <= now && t.EndDate >= now)
                .OrderByDescending(t => t.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SpecialDayTheme>> GetAllThemesAsync()
        {
            return await _context.SpecialDayThemes.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        public async Task<SpecialDayTheme> CreateThemeAsync(SpecialDayTheme theme)
        {
            theme.StartDate = DateTime.SpecifyKind(theme.StartDate, DateTimeKind.Utc);
            theme.EndDate = DateTime.SpecifyKind(theme.EndDate, DateTimeKind.Utc);
            _context.SpecialDayThemes.Add(theme);
            await _context.SaveChangesAsync();
            return theme;
        }

        public async Task UpdateThemeAsync(SpecialDayTheme theme)
        {
            theme.StartDate = DateTime.SpecifyKind(theme.StartDate, DateTimeKind.Utc);
            theme.EndDate = DateTime.SpecifyKind(theme.EndDate, DateTimeKind.Utc);
            _context.Entry(theme).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteThemeAsync(int id)
        {
            var theme = await _context.SpecialDayThemes.FindAsync(id);
            if (theme != null)
            {
                _context.SpecialDayThemes.Remove(theme);
                await _context.SaveChangesAsync();
            }
        }
    }
}
