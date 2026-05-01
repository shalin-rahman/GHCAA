using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class LookupItemConfiguration : IEntityTypeConfiguration<LookupItem>
    {
        public void Configure(EntityTypeBuilder<LookupItem> builder)
        {
            builder.HasKey(l => l.Id);

            builder.HasIndex(l => new { l.LookupGroup, l.Value }).IsUnique();
        }
    }
}
