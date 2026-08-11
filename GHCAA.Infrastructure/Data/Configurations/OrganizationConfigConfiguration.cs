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
            // App-managed concurrency token (NOT store-generated). SQL Server's rowversion is
            // auto-generated, but Npgsql/SQLite do not populate a physical bytea/BLOB column, so
            // IsRowVersion() left it NULL on insert (23502 not-null violation on Postgres). We mark
            // it a plain concurrency token and set a fresh value in OrgConfigService on every write,
            // giving real optimistic concurrency across all providers with no migration/xmin change.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();
            builder.Property(x => x.UpdatedByAdminId).HasMaxLength(50);
            builder.HasIndex(x => x.OrgId).IsUnique();
        }
    }
}
