using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace GHCAA.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(u => u.Username).IsUnique();

            // S8.1: Performance indexes for common lookup patterns.
            builder.HasIndex(u => u.MemberId);
            builder.HasIndex(u => u.ResetToken).HasFilter("\"ResetToken\" IS NOT NULL");
            builder.HasIndex(u => u.GoogleId).HasFilter("\"GoogleId\" IS NOT NULL");
            builder.HasIndex(u => u.FacebookId).HasFilter("\"FacebookId\" IS NOT NULL");

            builder.HasOne(u => u.Member)
                .WithOne(m => m.User)
                .HasForeignKey<User>(u => u.MemberId);

            builder.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RolesId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UsersId"),
                    j =>
                    {
                        j.HasKey("RolesId", "UsersId");
                        j.ToTable("UserRoles");
                    });

            builder.HasQueryFilter(u => !u.IsArchived);
        }
    }
}
