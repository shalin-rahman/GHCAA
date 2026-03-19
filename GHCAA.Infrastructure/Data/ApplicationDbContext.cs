using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public static bool IsSeedDisabled { get; set; }

        private static string GetSeedPath(string fileName) 
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seed", fileName);

        private List<T> LoadSeed<T>(string fileName)
        {
            // 1. Try local publish/output directory
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seed", fileName);
            
            // 2. Fallback to solution-relative path (for dev/migrations)
            if (!File.Exists(path))
            {
                var current = Directory.GetCurrentDirectory();
                path = Path.Combine(current, "GHCAA.Infrastructure", "Data", "Seed", fileName);
                
                // 3. Fallback if running from within Infrastructure project
                if (!File.Exists(path))
                    path = Path.Combine(current, "Data", "Seed", fileName);
            }

            if (!File.Exists(path)) return new List<T>();
            
            var json = File.ReadAllText(path);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

            // Strip collections to avoid EF Core HasData navigation errors
            var collectionProps = typeof(T).GetProperties()
                .Where(p => p.PropertyType != typeof(string) && 
                            typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType))
                .ToList();

            if (collectionProps.Any())
            {
                foreach (var item in items)
                {
                    foreach (var prop in collectionProps)
                    {
                        if (prop.CanWrite) prop.SetValue(item, null);
                    }
                }
            }

            return items;
        }

        public ApplicationDbContext(DbContextOptions options)
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
        public DbSet<MembershipDue> MembershipDues { get; set; } = null!;
        public DbSet<JobOpportunity> JobOpportunities { get; set; } = null!;
        public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
        public DbSet<EventGallery> EventGalleries { get; set; } = null!;
        public DbSet<EventPhoto> EventPhotos { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<MembershipFeeConfig> MembershipFeeConfigs { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AlumniEvent> AlumniEvents { get; set; } = null!;
        public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
        public DbSet<SpecialDayTheme> SpecialDayThemes { get; set; } = null!;
        public DbSet<EmailLog> EmailLogs { get; set; } = null!;
        public DbSet<ECPeriod> ECPeriods { get; set; } = null!;
        public DbSet<ECMember> ECMembers { get; set; } = null!;
        public DbSet<AcademicRecord> AcademicRecords { get; set; } = null!;
        public DbSet<ProfessionalRecord> ProfessionalRecords { get; set; } = null!;
        public DbSet<PaymentConfiguration> PaymentConfigurations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Force UTC for all DateTime properties (PostgreSQL requirement)
            var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var utcNullableConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(utcNullableConverter);
                    }
                }
            }

            // Member relationships
            modelBuilder.Entity<AcademicRecord>()
                .HasOne(a => a.Member)
                .WithMany(m => m.AcademicHistory)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProfessionalRecord>()
                .HasOne(p => p.Member)
                .WithMany(m => m.ProfessionalHistory)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentHistory>()
                .HasOne(p => p.Member)
                .WithMany(m => m.PaymentHistories)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Member <-> User (one-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Member)
                .WithOne(m => m.User)
                .HasForeignKey<User>(u => u.MemberId);

            // User <-> Role (many-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
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

            // LookupItem Unique Constraint
            modelBuilder.Entity<LookupItem>()
                .HasIndex(l => new { l.Category, l.Value }).IsUnique();

            modelBuilder.Entity<EmailTemplate>()
                .HasIndex(t => t.Code).IsUnique();

            // Seed Lookups from JSON
            if (IsSeedDisabled) return;

            var lookupItems = LoadSeed<LookupItem>("lookups.json");
            if (lookupItems.Any()) modelBuilder.Entity<LookupItem>().HasData(lookupItems);

            // Seed Email Templates
            // Seed Email Templates from JSON
            var emailTemplates = LoadSeed<EmailTemplate>("email_templates.json");
            if (emailTemplates.Any()) modelBuilder.Entity<EmailTemplate>().HasData(emailTemplates);

            // Soft Delete Filters
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsArchived);
            modelBuilder.Entity<Member>().HasQueryFilter(m => !m.IsArchived);
            
            // Apply matching filters to related entities to resolve CS8602-related architecture warnings
            modelBuilder.Entity<AcademicRecord>().HasQueryFilter(a => a.Member != null && !a.Member.IsArchived);
            modelBuilder.Entity<ProfessionalRecord>().HasQueryFilter(p => p.Member != null && !p.Member.IsArchived);
            modelBuilder.Entity<FileUpload>().HasQueryFilter(f => f.Member != null && !f.Member.IsArchived);
            modelBuilder.Entity<MembershipDue>().HasQueryFilter(d => d.Member != null && !d.Member.IsArchived);
            modelBuilder.Entity<MembershipHistory>().HasQueryFilter(h => h.Member != null && !h.Member.IsArchived);
            modelBuilder.Entity<Notification>().HasQueryFilter(n => n.Member != null && !n.Member.IsArchived);
            modelBuilder.Entity<PaymentHistory>().HasQueryFilter(ph => ph.Member != null && !ph.Member.IsArchived);
            modelBuilder.Entity<ECMember>().HasQueryFilter(em => em.Member != null && !em.Member.IsArchived);
            modelBuilder.Entity<NewsPost>().HasQueryFilter(np => np.Author != null && !np.Author.IsArchived);
            modelBuilder.Entity<ChatMessage>().HasQueryFilter(cm => (cm.Sender != null && !cm.Sender.IsArchived) && (cm.Receiver != null && !cm.Receiver.IsArchived));

            // Unique constraints
            modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.NID).IsUnique();
            modelBuilder.Entity<Member>().HasIndex(m => m.MobileNo).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            
            // Event Registration unique constraint (Member can only register once for an event)
            modelBuilder.Entity<EventRegistration>()
                .HasIndex(r => new { r.EventId, r.MemberId })
                .IsUnique()
                .HasFilter("\"MemberId\" IS NOT NULL");

            // Guest registration unique constraint
            modelBuilder.Entity<EventRegistration>()
                .HasIndex(r => new { r.EventId, r.GuestEmail })
                .IsUnique()
                .HasFilter("\"GuestEmail\" IS NOT NULL");

            // Payment uniqueness
            modelBuilder.Entity<PaymentHistory>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();
            
            // Seed Roles
            // Seed Roles from JSON
            var roles = LoadSeed<Role>("roles.json");
            if (roles.Any()) modelBuilder.Entity<Role>().HasData(roles);

            // Seed Members from JSON
            var members = LoadSeed<Member>("members.json");
            if (members.Any()) modelBuilder.Entity<Member>().HasData(members);

            // Seed users from JSON
            var users = LoadSeed<User>("users.json");
            if (users.Any()) modelBuilder.Entity<User>().HasData(users);

            // Seed Payment Histories
            var payments = LoadSeed<PaymentHistory>("payment_histories.json");
            if (payments.Any()) modelBuilder.Entity<PaymentHistory>().HasData(payments);

            // Seed many-to-many Roles for Users (UserRoles junction table)
            var userRoles = LoadSeed<Dictionary<string, object>>("user_roles.json");
            if (userRoles.Any())
            {
                modelBuilder.Entity("UserRoles").HasData(userRoles.Select(ur => new { 
                    RolesId = int.Parse(ur["RolesId"]?.ToString() ?? "0"), 
                    UsersId = int.Parse(ur["UsersId"]?.ToString() ?? "0") 
                }).ToList());
            }

            modelBuilder.Entity<FileUpload>()
                .HasIndex(f => new { f.MemberId, f.UploadType });

            modelBuilder.Entity<PaymentHistory>()
                .HasIndex(p => new { p.MemberId, p.TransactionId });

            modelBuilder.Entity<Member>().Property(m => m.FullName).HasMaxLength(200);
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<FileUpload>().Property(f => f.FileName).HasMaxLength(260);

            // ChatMessage Relationships
            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityLog Index
            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => new { a.MemberId, a.Timestamp });

            // Event Gallery Relationships
            modelBuilder.Entity<EventGallery>()
                .HasMany(g => g.Photos)
                .WithOne(p => p.EventGallery)
                .HasForeignKey(p => p.EventGalleryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Membership Fees from JSON
            var feeConfigs = LoadSeed<MembershipFeeConfig>("fee_configs.json");
            if (feeConfigs.Any()) modelBuilder.Entity<MembershipFeeConfig>().HasData(feeConfigs);

            // Seed Payment Configurations from JSON
            var paymentConfigs = LoadSeed<PaymentConfiguration>("payment_configurations.json");
            if (paymentConfigs.Any()) modelBuilder.Entity<PaymentConfiguration>().HasData(paymentConfigs);

            // Seed EC Period and Members from JSON
            var ecPeriods = LoadSeed<ECPeriod>("ec_periods.json");
            if (ecPeriods.Any()) modelBuilder.Entity<ECPeriod>().HasData(ecPeriods);

            var ecMembers = LoadSeed<ECMember>("ec_members.json");
            if (ecMembers.Any()) modelBuilder.Entity<ECMember>().HasData(ecMembers);

            // Seed News from JSON
            var news = LoadSeed<NewsPost>("news.json");
            if (news.Any()) modelBuilder.Entity<NewsPost>().HasData(news);

            // Seed Events from JSON
            var events = LoadSeed<AlumniEvent>("events.json");
            if (events.Any()) modelBuilder.Entity<AlumniEvent>().HasData(events);

            // Seed Jobs from JSON
            var jobs = LoadSeed<JobOpportunity>("jobs.json");
            if (jobs.Any()) modelBuilder.Entity<JobOpportunity>().HasData(jobs);

            // Seed Themes from JSON
            var themes = LoadSeed<SpecialDayTheme>("themes.json");
            if (themes.Any()) modelBuilder.Entity<SpecialDayTheme>().HasData(themes);

            // Seed Gallery and Photos from JSON
            var galleries = LoadSeed<EventGallery>("galleries.json");
            if (galleries.Any()) modelBuilder.Entity<EventGallery>().HasData(galleries);

            var photos = LoadSeed<EventPhoto>("photos.json");
            if (photos.Any()) modelBuilder.Entity<EventPhoto>().HasData(photos);

            // Seed Academic Records from JSON
            var academic = LoadSeed<AcademicRecord>("academic_records.json");
            if (academic.Any()) modelBuilder.Entity<AcademicRecord>().HasData(academic);

            // Seed Professional Records from JSON
            var professional = LoadSeed<ProfessionalRecord>("professional_records.json");
            if (professional.Any()) modelBuilder.Entity<ProfessionalRecord>().HasData(professional);

            // Seed Membership Histories from JSON
            var histories = LoadSeed<MembershipHistory>("membership_histories.json");
            if (histories.Any()) modelBuilder.Entity<MembershipHistory>().HasData(histories);

            // Seed Membership Dues from JSON
            var dues = LoadSeed<MembershipDue>("membership_dues.json");
            if (dues.Any()) modelBuilder.Entity<MembershipDue>().HasData(dues);

            // Seed Payment Histories from JSON
            var payments = LoadSeed<PaymentHistory>("payment_histories.json");
            if (payments.Any()) modelBuilder.Entity<PaymentHistory>().HasData(payments);

            // Seed Financial Records from JSON
            var financial = LoadSeed<FinancialRecord>("financial_records.json");
            if (financial.Any()) modelBuilder.Entity<FinancialRecord>().HasData(financial);

            // Seed File Uploads from JSON
            var uploads = LoadSeed<FileUpload>("file_uploads.json");
            if (uploads.Any()) modelBuilder.Entity<FileUpload>().HasData(uploads);
        }
    }
}