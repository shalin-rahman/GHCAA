using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class AlumniEventConfiguration : IEntityTypeConfiguration<AlumniEvent>
    {
        public void Configure(EntityTypeBuilder<AlumniEvent> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasQueryFilter(e => !e.IsArchived);
        }
    }
}
