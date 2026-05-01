using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class AcademicRecordConfiguration : IEntityTypeConfiguration<AcademicRecord>
    {
        public void Configure(EntityTypeBuilder<AcademicRecord> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Member)
                .WithMany(m => m.AcademicHistory)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(a => a.Member != null && !a.Member.IsArchived);
        }
    }
}
