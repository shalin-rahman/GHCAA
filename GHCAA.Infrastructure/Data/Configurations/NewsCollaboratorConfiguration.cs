using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class NewsCollaboratorConfiguration : IEntityTypeConfiguration<NewsCollaborator>
    {
        public void Configure(EntityTypeBuilder<NewsCollaborator> builder)
        {
            builder.HasKey(nc => new { nc.NewsPostId, nc.UserId });

            builder.HasOne(nc => nc.NewsPost)
                .WithMany(n => n.Collaborators)
                .HasForeignKey(nc => nc.NewsPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nc => nc.User)
                .WithMany()
                .HasForeignKey(nc => nc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Matches NewsPost's own HasQueryFilter(np => np.Author != null && !np.Author.IsArchived)
            // — without this, EF warns (10622) that a filtered-out post's required NewsPost
            // navigation is unreachable.
            builder.HasQueryFilter(nc => nc.NewsPost != null && nc.NewsPost.Author != null && !nc.NewsPost.Author.IsArchived);
        }
    }
}
