using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ECMemberConfiguration : IEntityTypeConfiguration<ECMember>
    {
        public void Configure(EntityTypeBuilder<ECMember> builder)
        {
            builder.HasKey(em => em.Id);

            builder.Property(em => em.EndReason).HasConversion<string>().HasMaxLength(20);
            builder.Property(em => em.EndNote).HasMaxLength(Constants.Governance.VacancyNoteMaxLength);

            // 95.2: Restrict, because a position with past terms is retired, never deleted.
            builder.HasOne(em => em.EcPosition).WithMany().HasForeignKey(em => em.Position).OnDelete(DeleteBehavior.Restrict);

            // 82.29: !em.IsArchived keeps a permanently-deleted (wrong-entry) row out of ordinary
            // reads, the same way PaymentHistoryConfiguration does for its own hard-delete path.
            // Admin views that need the deleted rows use IgnoreQueryFilters() deliberately.
            builder.HasQueryFilter(em => em.Member != null && !em.Member.IsArchived && !em.IsArchived);
        }
    }

    public class ECPeriodHandoverCheckConfiguration : IEntityTypeConfiguration<ECPeriodHandoverCheck>
    {
        public void Configure(EntityTypeBuilder<ECPeriodHandoverCheck> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.ECPeriodId, x.Item }).IsUnique();
            builder.HasOne<ECPeriod>().WithMany(p => p.HandoverChecks).HasForeignKey(x => x.ECPeriodId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<User>().WithMany().HasForeignKey(x => x.SetByUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
