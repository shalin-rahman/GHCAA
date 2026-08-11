using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ProfessionalRecordConfiguration : IEntityTypeConfiguration<ProfessionalRecord>
    {
        public void Configure(EntityTypeBuilder<ProfessionalRecord> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.Member)
                .WithMany(m => m.ProfessionalHistory)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(p => p.Member != null && !p.Member.IsArchived);
        }
    }
}
