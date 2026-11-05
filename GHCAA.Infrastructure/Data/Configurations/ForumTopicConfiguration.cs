using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class ForumTopicConfiguration : IEntityTypeConfiguration<ForumTopic>
    {
        public void Configure(EntityTypeBuilder<ForumTopic> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Author)
                .WithMany()
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // One thread per poll or meeting. Forum topics leave both columns null, which the index allows.
            builder.HasIndex(t => new { t.ContextType, t.ContextId }).IsUnique();

            // Filter out deleted/inactive and topics where author is archived. A context thread has no category.
            builder.HasQueryFilter(t => t.IsActive && (t.CategoryId == null || t.Category!.IsActive) && (t.Author == null || !t.Author.IsArchived));
        }
    }
}
