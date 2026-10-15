using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class SpecialDayThemeConfiguration : IEntityTypeConfiguration<SpecialDayTheme>
    {
        public void Configure(EntityTypeBuilder<SpecialDayTheme> builder)
        {
            // 62.56g: the config pack import matches themes on Title + StartDate, so the pair must be unique.
            builder.HasIndex(t => new { t.Title, t.StartDate }).IsUnique();
        }
    }
}
