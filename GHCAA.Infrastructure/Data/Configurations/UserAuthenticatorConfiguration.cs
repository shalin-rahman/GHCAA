using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class UserAuthenticatorConfiguration : IEntityTypeConfiguration<UserAuthenticator>
    {
        public void Configure(EntityTypeBuilder<UserAuthenticator> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.ProtectedSecret).IsRequired();
            builder.Property(a => a.RecoveryCodeHashes).IsRequired();
            builder.HasIndex(a => a.UserId).IsUnique();

            // Matches User's own archived filter, as RefreshTokenConfiguration does.
            builder.HasQueryFilter(a => a.User != null && !a.User.IsArchived);
        }
    }
}
