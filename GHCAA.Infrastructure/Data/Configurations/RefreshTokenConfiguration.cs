using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.TokenHash).HasMaxLength(64).IsRequired();
            builder.HasIndex(r => r.TokenHash).IsUnique();
            // Composite index for the common lookup: hash + not revoked + not expired.
            builder.HasIndex(r => new { r.UserId, r.IsRevoked });

            // Matches User's own HasQueryFilter(u => !u.IsArchived) — without this, EF warns
            // (10622) that an archived user's required User navigation is unreachable.
            builder.HasQueryFilter(r => r.User != null && !r.User.IsArchived);
        }
    }
}
