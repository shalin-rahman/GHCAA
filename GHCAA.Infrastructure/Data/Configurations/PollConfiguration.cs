using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class PollConfiguration : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.HasKey(p => p.Id);

            // Existing rows take Other when the column is added.
            builder.Property(p => p.Category).HasDefaultValue(Constants.Discussion.DefaultCategory);
            builder.HasIndex(p => p.Category);

            builder.HasQueryFilter(p => !p.IsArchived);
        }
    }
}
