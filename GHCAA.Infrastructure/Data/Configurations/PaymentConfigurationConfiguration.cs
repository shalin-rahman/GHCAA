using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class PaymentConfigurationConfiguration : IEntityTypeConfiguration<PaymentConfiguration>
    {
        public void Configure(EntityTypeBuilder<PaymentConfiguration> builder)
        {
            // 62.56g: Method alone is not unique because Credit Card has one row per gateway
            // (SSLCommerz, DGePay), so the key includes Gateway.
            builder.HasIndex(p => new { p.Method, p.Gateway }).IsUnique();
        }
    }
}
