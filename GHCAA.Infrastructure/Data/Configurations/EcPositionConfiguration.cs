using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations;

public sealed class EcPositionConfiguration : IEntityTypeConfiguration<EcPosition>
{
    public void Configure(EntityTypeBuilder<EcPosition> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(Constants.Governance.PositionKeyMaxLength).IsRequired();
        b.Property(x => x.Name).HasMaxLength(Constants.Governance.PositionNameMaxLength).IsRequired();
        b.Property(x => x.NameBn).HasMaxLength(Constants.Governance.PositionNameMaxLength);
        b.HasIndex(x => x.Key).IsUnique();
    }
}
