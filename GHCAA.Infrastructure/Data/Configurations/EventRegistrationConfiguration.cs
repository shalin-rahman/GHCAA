using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
    {
        public void Configure(EntityTypeBuilder<EventRegistration> builder)
        {
            builder.HasKey(r => r.Id);

            // Member can only register once for an event
            builder.HasIndex(r => new { r.EventId, r.MemberId })
                .IsUnique()
                .HasFilter("\"MemberId\" IS NOT NULL");

            // Guest registration unique constraint
            builder.HasIndex(r => new { r.EventId, r.GuestEmail })
                .IsUnique()
                .HasFilter("\"GuestEmail\" IS NOT NULL");

            // Matches AlumniEvent's own HasQueryFilter(e => e.IsActive) — without this, EF warns
            // (10622) that an inactive event's required Event navigation is unreachable.
            builder.HasQueryFilter(r => r.Event != null && r.Event.IsActive);
        }
    }
}
