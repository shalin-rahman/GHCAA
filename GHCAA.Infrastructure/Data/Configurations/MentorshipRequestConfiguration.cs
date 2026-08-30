using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class MentorshipRequestConfiguration : IEntityTypeConfiguration<MentorshipRequest>
    {
        public void Configure(EntityTypeBuilder<MentorshipRequest> builder)
        {
            builder.HasKey(m => m.Id);

            // Requester/Mentor relationships are convention-configured (RequesterId/MentorId FKs);
            // this class exists solely to add the query filter below, without touching the
            // existing convention-derived delete behavior.

            // Matches Member's own HasQueryFilter(m => !m.IsArchived) — without this, EF warns
            // (10622) that an archived requester/mentor's required navigation is unreachable.
            builder.HasQueryFilter(m => m.Requester != null && !m.Requester.IsArchived
                && m.Mentor != null && !m.Mentor.IsArchived);
        }
    }
}
