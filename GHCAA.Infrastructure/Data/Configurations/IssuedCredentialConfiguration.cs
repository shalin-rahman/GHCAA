using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public sealed class IssuedCredentialConfiguration : IEntityTypeConfiguration<IssuedCredential>
    {
        public void Configure(EntityTypeBuilder<IssuedCredential> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ShortCode).HasMaxLength(10).IsRequired();
            builder.HasIndex(x => x.ShortCode).IsUnique();
            builder.Property(x => x.RevokedReason).HasMaxLength(500);
            // Matches Member's own filter. An archived member's credential stops verifying at
            // /verify/{shortCode}, for every credential type (decided 2026-10-12, TODO 94.4).
            builder.HasQueryFilter(x => x.Member != null && !x.Member.IsArchived);
            builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
