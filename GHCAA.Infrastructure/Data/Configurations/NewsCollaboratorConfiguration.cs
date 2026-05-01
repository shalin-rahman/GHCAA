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
        }
    }
}
