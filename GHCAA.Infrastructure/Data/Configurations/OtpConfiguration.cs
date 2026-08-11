using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class OtpConfiguration : IEntityTypeConfiguration<Otp>
    {
        public void Configure(EntityTypeBuilder<Otp> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Email).HasMaxLength(254).IsRequired();
            // 24.20: Code stores HMAC-SHA256 hex (64 chars), not the plaintext 6-digit value.
            builder.Property(o => o.Code).HasMaxLength(64).IsRequired();

            // Composite index used by VerifyOtpAsync — email + expiry filter.
            builder.HasIndex(o => new { o.Email, o.ExpiryAt });
        }
    }
}
