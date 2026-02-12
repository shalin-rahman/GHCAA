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
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<LookupItem> Lookups { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.Otp> Otps { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.PaymentHistory> PaymentHistories { get; set; } = null!;
        public DbSet<GHCAA.Domain.Models.MembershipHistory> MembershipHistories { get; set; } = null!;
        public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;
        public DbSet<FinancialRecord> FinancialRecords { get; set; } = null!;
        public DbSet<NewsPost> NewsPosts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Member <-> User (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Member)
                .WithOne(m => m.User)
                .HasForeignKey<User>(u => u.MemberId);

            // User <-> Role (many-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            // LookupItem Unique Constraint
            modelBuilder.Entity<LookupItem>()
                .HasIndex(l => new { l.Category, l.Value }).IsUnique();

            modelBuilder.Entity<EmailTemplate>()
                .HasIndex(t => t.Code).IsUnique();

            // Seed Lookups
            modelBuilder.Entity<LookupItem>().HasData(
                new LookupItem { Id = 1, Category = "Degree", Value = "HSC", Label = "HSC", DisplayOrder = 1 },
                new LookupItem { Id = 2, Category = "Degree", Value = "Bachelor", Label = "Bachelor / Honours", DisplayOrder = 2 },
                new LookupItem { Id = 3, Category = "Degree", Value = "Masters", Label = "Masters", DisplayOrder = 3 },
                new LookupItem { Id = 4, Category = "Degree", Value = "PhD", Label = "PhD", DisplayOrder = 4 },
                new LookupItem { Id = 5, Category = "Degree", Value = "Other", Label = "Other", DisplayOrder = 5 },

                new LookupItem { Id = 6, Category = "ProfessionalSector", Value = "Teaching", Label = "Teaching / Education", DisplayOrder = 1 },
                new LookupItem { Id = 7, Category = "ProfessionalSector", Value = "Business", Label = "Business", DisplayOrder = 2 },
                new LookupItem { Id = 8, Category = "ProfessionalSector", Value = "IT", Label = "Information Technology", DisplayOrder = 3 },
                new LookupItem { Id = 9, Category = "ProfessionalSector", Value = "Medical", Label = "Medical / Healthcare", DisplayOrder = 4 },
                new LookupItem { Id = 10, Category = "ProfessionalSector", Value = "Government", Label = "Government Service", DisplayOrder = 5 },
                new LookupItem { Id = 11, Category = "ProfessionalSector", Value = "Other", Label = "Other", DisplayOrder = 6 }
            );

            // Seed Email Templates
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate 
                { 
                    Id = 1, 
                    Code = "OTP_EMAIL", 
                    Subject = "Your GHC Alumni Association Verification Code", 
                    Body = "Hello {{FullName}}, your OTP is: <strong>{{OtpCode}}</strong>. Valid for 10 minutes.",
                    Variables = "['FullName', 'OtpCode']"
                },
                new EmailTemplate 
                { 
                    Id = 2, 
                    Code = "WELCOME_EMAIL", 
                    Subject = "Welcome to GHC Alumni Association!", 
                    Body = "Dear {{FullName}}, welcome! Your membership number is {{MembershipNumber}}.",
                    Variables = "['FullName', 'MembershipNumber']"
                }
            );

            // Soft Delete Filters
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsArchived);
            modelBuilder.Entity<Member>().HasQueryFilter(m => !m.IsArchived);

            // Unique constraints
            modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.NID).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.MobileNo).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "SuperAdmin" },
                new Role { Id = 2, Name = "Admin" },
                new Role { Id = 3, Name = "Member" }
            );

            // Seed Super Admin
            // Password: SuperAdminPassword123!
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "superadmin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdminPassword123!"),
                IsActive = true,
                IsArchived = false,
                CreatedAt = new DateTime(2024, 1, 1),
                MemberId = 0 // SuperAdmin might not be a member
            });

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