using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<FileUpload> FileUploads { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.Otp> Otps { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.PaymentHistory> PaymentHistories { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.MembershipHistory> MembershipHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Member <-> User (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Member)
                .WithOne(m => m.User)
                .HasForeignKey<User>(u => u.MemberId);

            // Unique constraints
            modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.NID).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.MobileNo).IsUnique();
            
            modelBuilder.Entity<FileUpload>()
                .HasIndex(f => new { f.MemberId, f.UploadType });

            modelBuilder.Entity<PaymentHistory>()
                .HasIndex(p => new { p.MemberId, p.TransactionId });

            modelBuilder.Entity<Member>().Property(m => m.FullName).HasMaxLength(200);
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<FileUpload>().Property(f => f.FileName).HasMaxLength(260);
        }
    }
}