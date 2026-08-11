using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FullName)
                .HasMaxLength(200);

            builder.HasIndex(m => m.Email).IsUnique();
            builder.HasIndex(m => m.NID).IsUnique();
            builder.HasIndex(m => m.MobileNo).IsUnique();

            // 24.37: Admin member listing queries filter heavily on both Status and IsArchived.
            builder.HasIndex(m => new { m.Status, m.IsArchived });

            builder.HasQueryFilter(m => !m.IsArchived);
        }
    }
}
