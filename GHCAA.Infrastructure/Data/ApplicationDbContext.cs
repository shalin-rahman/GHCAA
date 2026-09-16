using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
    {
        public static bool IsSeedDisabled { get; set; }

        private static string GetSeedPath(string fileName)
            => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seed", fileName);

        // Tier 3 seed files per docs/SEED_CLASSIFICATION.md — one institution's own members,
        // events and payment history, not something every institution starts with. These live
        // under profiles/<name>/demo-data/ instead of Data/Seed/. This list must match
        // InstitutionDataSeeder.Registry exactly — that registry, not this set, is the source of
        // truth for which files are Tier 3; see docs/SEED_CLASSIFICATION.md.
        private static readonly HashSet<string> DemoDataFiles = new(StringComparer.OrdinalIgnoreCase)
        {
            "members.json", "users.json", "user_roles.json", "ec_members.json", "ec_periods.json",
            "events.json", "galleries.json", "news.json",
            "academic_records.json", "professional_records.json", "photos.json",
            "payment_histories.json", "jobs.json", "saved_payment_methods.json",
        };

        // Walks up from a starting directory looking for a "profiles" folder. Needed because
        // AppDomain.CurrentDomain.BaseDirectory and Directory.GetCurrentDirectory() can both land
        // several levels below the repo root — e.g. dotnet test's working directory is the test
        // project's bin/Debug/net9.0, five levels down from where profiles/ actually lives, and a
        // single parent-directory check (as GetSeedPath's Data/Seed fallback uses) isn't enough.
        private static string? FindProfilesRoot(string startDir)
        {
            var dir = startDir;
            for (var i = 0; i < 6 && dir != null; i++)
            {
                var candidate = Path.Combine(dir, "profiles");
                if (Directory.Exists(candidate)) return candidate;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return null;
        }

        // Same fallback shape as InstitutionProfileProvider: try the selected institution profile,
        // then fall back to the "default" pack, file-by-file, checking both the published output
        // directory and the solution-relative path a local dev run/migration uses.
        private static string? ResolveDemoDataPath(string fileName)
        {
            var orgProfile = Environment.GetEnvironmentVariable("ORG_PROFILE");
            var profileName = string.IsNullOrWhiteSpace(orgProfile) ? "default" : orgProfile;
            var namesToTry = profileName == "default" ? new[] { "default" } : new[] { profileName, "default" };

            var profilesRoot = FindProfilesRoot(AppDomain.CurrentDomain.BaseDirectory)
                ?? FindProfilesRoot(Directory.GetCurrentDirectory());
            if (profilesRoot == null) return null;

            foreach (var name in namesToTry)
            {
                var path = Path.Combine(profilesRoot, name, "demo-data", fileName);
                if (File.Exists(path)) return path;
            }

            return null;
        }

        /// <summary>
        /// The demo-data folder the active ORG_PROFILE would resolve to, if the profiles tree can
        /// be found at all. Used by <see cref="InstitutionDataSeeder"/> to warn about a Tier 3 file
        /// sitting on disk with no entry in its registry — the same drift that let 62.32 miss
        /// users.json/user_roles.json the first time.
        /// </summary>
        internal static string? FindActiveDemoDataDirectory()
        {
            var orgProfile = Environment.GetEnvironmentVariable("ORG_PROFILE");
            var profileName = string.IsNullOrWhiteSpace(orgProfile) ? "default" : orgProfile;

            var profilesRoot = FindProfilesRoot(AppDomain.CurrentDomain.BaseDirectory)
                ?? FindProfilesRoot(Directory.GetCurrentDirectory());
            if (profilesRoot == null) return null;

            var path = Path.Combine(profilesRoot, profileName, "demo-data");
            return Directory.Exists(path) ? path : null;
        }

        // Tier 1/2 seed files per docs/SEED_CLASSIFICATION.md — institution-specific content a
        // profile pack may want to supply its own copy of (site content, themes, constitution).
        // Unlike demo-data these live directly under profiles/<name>/, not a demo-data subfolder.
        // No profile pack ships its own copy of any of these yet, so this always falls through to
        // the existing Data/Seed/ file below — it only starts mattering once WP62.33-62.36 add
        // per-profile copies.
        //
        // lookups.json is deliberately NOT here: dropdown categories are shared application
        // taxonomy, the same set for every institution, not something a profile should be able to
        // override — it always loads from Data/Seed/lookups.json regardless of ORG_PROFILE.
        private static readonly HashSet<string> ProfilePackFiles = new(StringComparer.OrdinalIgnoreCase)
        {
            "email_templates.json", "site_content.json", "themes.json", "constitution.json",
        };

        private static string? ResolveProfilePackPath(string fileName)
        {
            var orgProfile = Environment.GetEnvironmentVariable("ORG_PROFILE");
            var profileName = string.IsNullOrWhiteSpace(orgProfile) ? "default" : orgProfile;
            var namesToTry = profileName == "default" ? new[] { "default" } : new[] { profileName, "default" };

            var profilesRoot = FindProfilesRoot(AppDomain.CurrentDomain.BaseDirectory)
                ?? FindProfilesRoot(Directory.GetCurrentDirectory());
            if (profilesRoot == null) return null;

            foreach (var name in namesToTry)
            {
                var path = Path.Combine(profilesRoot, name, fileName);
                if (File.Exists(path)) return path;
                // No alt-name substitution here. Profile pack seed files must use the same
                // underscore-based canonical name as the LoadSeed<T> call site.  The hyphen
                // variant of some names (e.g. site-content.json) is an Angular build-time
                // branding file — a flat object, not a seed array — and must not shadow the
                // Data/Seed/ copy that LoadSeed<T> falls back to.
            }

            return null;

        }

        /// <summary>
        /// Resolves and deserializes a Seed JSON file. Static and internal so runtime data
        /// syncers (e.g. <see cref="ConstitutionSeeder"/>) reuse the exact same profile gate and
        /// path-resolution order instead of duplicating it.
        /// </summary>
        internal static List<T> LoadSeed<T>(string fileName)
        {
            var profile = Environment.GetEnvironmentVariable("ASP_SEED_PROFILE");

            // SECURITY GATE: Never allow 'Visual' profile during migration generation or if not explicitly requested.
            // Keeps test data (Shalin Rahman, etc.) out of the production database snapshot.
            // EF.IsDesignTime is true only inside `dotnet ef`. The old assembly-scan heuristic also
            // fired during ordinary test runs (the test assembly references the Design package
            // transitively), which routed Visual-profile test factories through the Tier 3
            // profile-pack path instead of Seed/Visual once real member data moved out of Data/Seed
            // into profiles/ghc/demo-data — breaking the FamilyLinkRequests fallback that expects
            // Member 200/201 to exist.
            var isDesign = EF.IsDesignTime;

            var seedSubDir = (profile == "Visual" && !isDesign) ? "Seed/Visual" : "Seed";

            // Visual-profile fixtures are e2e test data, not an institution's own history, so they
            // stay in Seed/Visual untouched rather than routing through the profile pack.
            string? path = null;
            if (seedSubDir != "Seed/Visual")
            {
                if (DemoDataFiles.Contains(fileName)) path = ResolveDemoDataPath(fileName);
                else if (ProfilePackFiles.Contains(fileName)) path = ResolveProfilePackPath(fileName);
            }

            if (path == null)
            {
                // 1. Try local publish/output directory
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", seedSubDir, fileName);

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
            }

            return LoadSeedFromPath<T>(path);
        }

        /// <summary>
        /// Reads and deserializes one seed file at an already-resolved path. Shared tail end of
        /// <see cref="LoadSeed{T}"/>, and also used directly by <see cref="InstitutionDataSeeder"/> for
        /// an operator-supplied real-data override path that bypasses profile resolution entirely.
        /// </summary>
        internal static List<T> LoadSeedFromPath<T>(string path)
        {
            if (!File.Exists(path)) return new List<T>();

            Console.WriteLine($"[SEED] Loading from: {path}");
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
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignPledge> CampaignPledges { get; set; } = null!;
        public DbSet<DonorRecognitionTier> DonorRecognitionTiers { get; set; } = null!;
        public DbSet<AmendmentVote> AmendmentVotes { get; set; } = null!;
        public DbSet<MentorshipRequest> MentorshipRequests { get; set; } = null!;
        public DbSet<SocialAuthConfig> SocialAuthConfigs { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<PollOption> PollOptions { get; set; } = null!;
        public DbSet<PollVote> PollVotes { get; set; } = null!;

        // Forum Module
        public DbSet<ForumCategory> ForumCategories { get; set; } = null!;
        public DbSet<ForumTopic> ForumTopics { get; set; } = null!;
        public DbSet<ForumPost> ForumPosts { get; set; } = null!;

        // Config-Driven Framework
        public DbSet<OrganizationConfig> OrganizationConfigs { get; set; } = null!;

        // 45.2: unhandled exceptions captured by ExceptionMiddleware.
        public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
        public DbSet<SiteContent> SiteContents { get; set; } = null!;

        // Data Protection keys persisted here rather than to the container filesystem — Render
        // has no persistent disk mounted on this service, so a file-based key ring (the ASP.NET
        // Core default, or the old KeyRingPath config) silently regenerates on every redeploy,
        // invalidating anything it protects. The database already survives redeploys.
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;


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

            // The two Visual-profile FamilyLinkRequest rows (Member 200 <-> 1 and 200 <-> 2) used
            // to be seeded here via HasData. HasData runs as part of EnsureCreated's own schema
            // script, before DatabaseBootstrapperExtensions ever gets a chance to load Members from
            // members.json, so the FK to Members always failed on a fresh Visual database. They're
            // seeded programmatically in DatabaseBootstrapperExtensions.BootstrapDatabaseAsync
            // instead, after Members exist.

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

            // Members, Users and UserRoles are Tier 3 (docs/SEED_CLASSIFICATION.md) — one
            // institution's own accounts, not schema. Seeded at runtime by InstitutionDataSeeder
            // instead of HasData, so this data never lands in a committed migration again.

            // Seed Membership Fees from JSON
            var feeConfigs = LoadSeed<MembershipFeeConfig>("fee_configs.json");
            if (feeConfigs.Any()) modelBuilder.Entity<MembershipFeeConfig>().HasData(feeConfigs);

            // Seed Payment Configurations from JSON
            var paymentConfigs = LoadSeed<PaymentConfiguration>("payment_configurations.json");
            if (paymentConfigs.Any()) modelBuilder.Entity<PaymentConfiguration>().HasData(paymentConfigs);

            // EC Periods/Members, News, Events and Jobs are Tier 3 — seeded at runtime by
            // InstitutionDataSeeder, not HasData (see the note above Members).

            // Seed Themes from JSON
            var themes = LoadSeed<SpecialDayTheme>("themes.json");
            if (themes.Any()) modelBuilder.Entity<SpecialDayTheme>().HasData(themes);

            // Galleries, Photos, Academic Records and Professional Records are Tier 3 —
            // seeded at runtime by InstitutionDataSeeder, not HasData (see the note above Members).

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

            // Saved Payment Methods are Tier 3 — seeded at runtime by InstitutionDataSeeder, not
            // HasData (see the note above Members).

            // Seed CMS content blocks (About/Contact) from JSON
            var siteContent = LoadSeed<SiteContent>("site_content.json");
            if (siteContent.Any()) modelBuilder.Entity<SiteContent>().HasData(siteContent);
        }
    }
}