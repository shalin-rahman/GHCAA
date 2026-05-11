using GHCAA.Domain;
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
            var profile = Environment.GetEnvironmentVariable("ASP_SEED_PROFILE");
            
            // SECURITY GATE: Never allow 'Visual' profile during migration generation or if not explicitly requested.
            // This ensures test data (Shalin Rahman, etc.) never ends up in the production database snapshot.
            var isDesign = AppDomain.CurrentDomain.FriendlyName.Contains("ef") || 
                           AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.Contains("Microsoft.EntityFrameworkCore.Design") == true);

            var seedSubDir = (profile == "Visual" && !isDesign) ? "Seed/Visual" : "Seed";

            // 1. Try local publish/output directory
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", seedSubDir, fileName);
            
            // 2. Fallback to base Seed if Visual missing
            if (profile == "Visual" && !File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seed", fileName);
            }

            // 3. Fallback to solution-relative path (for dev/migrations)
            if (!File.Exists(path))
            {
                var current = Directory.GetCurrentDirectory();
                var infrastructurePath = Path.Combine(current, "GHCAA.Infrastructure");
                
                // If we are running from GHCAA.API, look in parent
                if (!Directory.Exists(infrastructurePath))
                {
                    var parent = Directory.GetParent(current)?.FullName;
                    if (parent != null) infrastructurePath = Path.Combine(parent, "GHCAA.Infrastructure");
                }

                var relPath = Path.Combine(infrastructurePath, "Data", seedSubDir, fileName);
                
                if (profile == "Visual" && !File.Exists(relPath))
                    relPath = Path.Combine(infrastructurePath, "Data", "Seed", fileName);

                path = relPath;
            }

            if (!File.Exists(path)) return new List<T>();
            
            Console.WriteLine($"[SEED] Loading {fileName} from: {path}");
            var json = File.ReadAllText(path);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

            // Strip collections to avoid EF Core HasData navigation errors, but preserve List<string> which map to native PG arrays
            var collectionProps = typeof(T).GetProperties()
                .Where(p => p.PropertyType != typeof(string) && 
                            p.PropertyType != typeof(List<string>) &&
                            p.PropertyType != typeof(string[]) &&
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
        public DbSet<FamilyLinkRequest> FamilyLinkRequests { get; set; } = null!;
        public DbSet<EventTask> EventTasks { get; set; } = null!;
        public DbSet<EventBudget> EventBudgets { get; set; } = null!;
        public DbSet<EventExpense> EventExpenses { get; set; } = null!;
        public DbSet<GamificationConfig> GamificationConfigs { get; set; } = null!;
        public DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; } = null!;
        public DbSet<NewsCollaborator> NewsCollaborators { get; set; } = null!;
        public DbSet<Constitution> Constitutions { get; set; } = null!;
        public DbSet<AmendmentVote> AmendmentVotes { get; set; } = null!;
        public DbSet<MentorshipRequest> MentorshipRequests { get; set; } = null!;
        public DbSet<SocialAuthConfig> SocialAuthConfigs { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<PollOption> PollOptions { get; set; } = null!;
        public DbSet<PollVote> PollVotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            Console.WriteLine($"[SEED] Profile: {Environment.GetEnvironmentVariable("ASP_SEED_PROFILE")}");

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

            // Seed Lookups from JSON
            if (IsSeedDisabled) return;

            var lookupItems = LoadSeed<LookupItem>("lookups.json");
            if (lookupItems.Any()) modelBuilder.Entity<LookupItem>().HasData(lookupItems);

            // Seed Email Templates from JSON
            var emailTemplates = LoadSeed<EmailTemplate>("email_templates.json");
            if (emailTemplates.Any()) modelBuilder.Entity<EmailTemplate>().HasData(emailTemplates);

            // Seed Family Link Requests from JSON
            var familyLinks = LoadSeed<FamilyLinkRequest>("family_links.json");
            if (familyLinks.Any()) modelBuilder.Entity<FamilyLinkRequest>().HasData(familyLinks);

            // PROGRAMMATIC SEEDING FOR VISUAL TEST PROFILE
            var profile = Environment.GetEnvironmentVariable("ASP_SEED_PROFILE");
            var isDesign = AppDomain.CurrentDomain.FriendlyName.Contains("ef") || 
                           AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName?.Contains("Microsoft.EntityFrameworkCore.Design") == true);

            if (profile == "Visual" && !isDesign && !familyLinks.Any())
            {
                modelBuilder.Entity<FamilyLinkRequest>().HasData(
                    new FamilyLinkRequest 
                    { 
                        Id = 9991, 
                        RequesterId = 200, 
                        TargetMemberId = 1, 
                        Status = Enums.FamilyLinkStatus.Accepted, 
                        Relationship = Enums.RelationshipType.Other, 
                        RequestedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
                    },
                    new FamilyLinkRequest 
                    { 
                        Id = 9992, 
                        RequesterId = 2, 
                        TargetMemberId = 200, 
                        Status = Enums.FamilyLinkStatus.Accepted, 
                        Relationship = Enums.RelationshipType.Other, 
                        RequestedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
                    }
                );
            }

            // Seed Gamification Configs
            modelBuilder.Entity<GamificationConfig>().HasData(
                new GamificationConfig { Id = 1, ActivityCode = "PROFILE_VERIFIED", Name = "Verifying Profile", Points = 50 },
                new GamificationConfig { Id = 2, ActivityCode = "EVENT_ATTENDANCE", Name = "Attending an Event", Points = 100 },
                new GamificationConfig { Id = 3, ActivityCode = "DONATION", Name = "Making a Donation", Points = 200 },
                new GamificationConfig { Id = 4, ActivityCode = "MENTORING", Name = "Mentoring a Fellow Alumni", Points = 200 }
            );

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

            // Seed Financial Records from JSON
            var financial = LoadSeed<FinancialRecord>("financial_records.json");
            if (financial.Any()) modelBuilder.Entity<FinancialRecord>().HasData(financial);

            // Seed File Uploads from JSON
            var uploads = LoadSeed<FileUpload>("file_uploads.json");
            if (uploads.Any()) modelBuilder.Entity<FileUpload>().HasData(uploads);

            // Seed Constitutions from JSON
            var constitutions = LoadSeed<Constitution>("constitution.json");
            if (constitutions.Any()) modelBuilder.Entity<Constitution>().HasData(constitutions);

            // Seed Saved Payment Methods from JSON
            var savedMethods = LoadSeed<SavedPaymentMethod>("saved_payment_methods.json");
            if (savedMethods.Any()) modelBuilder.Entity<SavedPaymentMethod>().HasData(savedMethods);
        }
    }
}