using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class OrganizationConfigConfiguration : IEntityTypeConfiguration<OrganizationConfig>
    {
        public void Configure(EntityTypeBuilder<OrganizationConfig> builder)
        {
            builder.ToTable("OrganizationConfigs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OrgId).HasMaxLength(50).IsRequired();
            // "text" works on PostgreSQL, SQLite, and MySQL — no JSONB-specific operations are needed here
            builder.Property(x => x.ConfigJson).HasColumnType("text").IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.Property(x => x.UpdatedByAdminId).HasMaxLength(50);
            builder.HasIndex(x => x.OrgId).IsUnique();
        }
    }
}
