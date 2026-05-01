using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class EventGalleryConfiguration : IEntityTypeConfiguration<EventGallery>
    {
        public void Configure(EntityTypeBuilder<EventGallery> builder)
        {
            builder.HasKey(g => g.Id);

            builder.HasMany(g => g.Photos)
                .WithOne(p => p.EventGallery)
                .HasForeignKey(p => p.EventGalleryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
