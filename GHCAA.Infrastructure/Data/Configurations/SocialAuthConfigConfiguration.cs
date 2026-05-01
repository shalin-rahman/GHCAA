using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class SocialAuthConfigConfiguration : IEntityTypeConfiguration<SocialAuthConfig>
    {
        public void Configure(EntityTypeBuilder<SocialAuthConfig> builder)
        {
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.Provider).IsUnique();
        }
    }
}
