using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GHCAA.Infrastructure.Services
{
    public interface IThemeService
    {
        Task<SpecialDayTheme?> GetActiveThemeAsync();
        Task<List<SpecialDayTheme>> GetAllThemesAsync();
        Task<SpecialDayTheme> CreateThemeAsync(SpecialDayThemeSaveDto dto);
        // False when no theme has this id.
        Task<bool> UpdateThemeAsync(int id, SpecialDayThemeSaveDto dto);
        Task DeleteThemeAsync(int id);
    }

    public class ThemeService : IThemeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "GHCAA_ActiveTheme";
        private DateTime GetBangladeshTimeNow()
        {
            var utcNow = DateTime.UtcNow;
            try { return TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka")); }
            catch (TimeZoneNotFoundException)
            {
                try { return TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")); }
                catch (TimeZoneNotFoundException) { return utcNow.AddHours(6); }
            }
        }

        public ThemeService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<SpecialDayTheme?> GetActiveThemeAsync()
        {
            if (_cache.TryGetValue(CacheKey, out SpecialDayTheme? cachedTheme))
            {
                return cachedTheme;
            }

            // Refined Timezone Logic: Ensure theme activation matches Association's Local Time (GMT+6)
            var nowLocal = GetBangladeshTimeNow().Date;

            var theme = await _context.SpecialDayThemes
                .Where(t => t.IsEnabled && t.StartDate.Date <= nowLocal && t.EndDate.Date >= nowLocal)
                .OrderByDescending(t => t.StartDate)
                .FirstOrDefaultAsync();

            // Cache for 5 minutes to reduce DB hits on every public page load
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(CacheKey, theme, cacheOptions);

            return theme;
        }

        public async Task<List<SpecialDayTheme>> GetAllThemesAsync()
        {
            return await _context.SpecialDayThemes.OrderByDescending(t => t.StartDate).ToListAsync();
        }

        public async Task<SpecialDayTheme> CreateThemeAsync(SpecialDayThemeSaveDto dto)
        {
            await EnsureKeyFreeAsync(dto, excludeId: null);
            _cache.Remove(CacheKey); // Invalidate cache on change
            var theme = new SpecialDayTheme();
            Apply(dto, theme);
            _context.SpecialDayThemes.Add(theme);
            await _context.SaveChangesAsync();
            return theme;
        }

        public async Task<bool> UpdateThemeAsync(int id, SpecialDayThemeSaveDto dto)
        {
            var theme = await _context.SpecialDayThemes.FindAsync(id);
            if (theme == null) return false;

            // Only when the key moves, so a row left sharing its key from before the index stays
            // editable.
            if (theme.Title != dto.Title || theme.StartDate != dto.StartDate)
                await EnsureKeyFreeAsync(dto, excludeId: id);
            _cache.Remove(CacheKey); // Invalidate cache on change
            Apply(dto, theme);
            await _context.SaveChangesAsync();
            return true;
        }

        // 62.56g: Title + StartDate is unique (IX_SpecialDayThemes_Title_StartDate). Checked here so
        // the admin gets a 400 with a reason instead of a 500 from the index.
        private async Task EnsureKeyFreeAsync(SpecialDayThemeSaveDto dto, int? excludeId)
        {
            var taken = await _context.SpecialDayThemes.AnyAsync(t =>
                t.Title == dto.Title && t.StartDate == dto.StartDate && t.Id != excludeId);
            if (taken)
                throw new DuplicateKeyException(
                    $"A theme titled '{dto.Title}' starting on {dto.StartDate:yyyy-MM-dd} already exists.");
        }

        private static void Apply(SpecialDayThemeSaveDto dto, SpecialDayTheme theme)
        {
            theme.Title = dto.Title;
            theme.StartDate = dto.StartDate;
            theme.EndDate = dto.EndDate;
            theme.BackgroundColor = dto.BackgroundColor;
            theme.TextColor = dto.TextColor;
            theme.AnnouncementText = dto.AnnouncementText ?? string.Empty;
            theme.AnimationStyle = string.IsNullOrWhiteSpace(dto.AnimationStyle) ? "Fade" : dto.AnimationStyle;
            theme.ImageUrl = dto.ImageUrl ?? string.Empty;
            theme.SidebarColor = dto.SidebarColor ?? string.Empty;
            theme.EnableGradientFading = dto.EnableGradientFading;
            theme.IsEnabled = dto.IsEnabled;
        }

        public async Task DeleteThemeAsync(int id)
        {
            _cache.Remove(CacheKey); // Invalidate cache on change
            var theme = await _context.SpecialDayThemes.FindAsync(id);
            if (theme != null)
            {
                _context.SpecialDayThemes.Remove(theme);
                await _context.SaveChangesAsync();
            }
        }
    }
}
