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
            // EF warning 10622 asks for a filter on Member.IsArchived. Adding it means an archived member's
            // credential stops verifying at /verify/{shortCode}. Decide that per credential type first.
            // TODO 94.4.
            builder.HasOne(x => x.Member).WithMany().HasForeignKey(x => x.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
