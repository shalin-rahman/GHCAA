using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class SiteContentConfiguration : IEntityTypeConfiguration<SiteContent>
    {
        public void Configure(EntityTypeBuilder<SiteContent> builder)
        {
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.Key).IsUnique();
        }
    }
}
