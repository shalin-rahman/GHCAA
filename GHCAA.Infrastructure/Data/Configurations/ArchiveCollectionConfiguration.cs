using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ArchiveCollectionConfiguration : IEntityTypeConfiguration<ArchiveCollection>
    {
        public void Configure(EntityTypeBuilder<ArchiveCollection> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.HasMany(x => x.Items).WithOne(x => x.Collection)
                .HasForeignKey(x => x.ArchiveCollectionId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
