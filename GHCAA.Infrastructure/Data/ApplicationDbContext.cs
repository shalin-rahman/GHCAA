using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
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

            // Seed Lookups
            int lookId = 1;
            var lookupItems = new List<LookupItem>();

            // Degrees
            foreach (var d in new[] { "HSC", "Bachelor (Pass)", "Bachelor (Honours)", "Masters", "PGD", "PhD", "Medicine", "Engineering", "Law" })
                lookupItems.Add(new LookupItem { Id = lookId++, Category = "Degree", Value = d, Label = d, DisplayOrder = lookId });

            // Groups
            foreach (var g in new[] { "Science", "Arts & Humanities", "Business Studies" })
                lookupItems.Add(new LookupItem { Id = lookId++, Category = "AcademicGroup", Value = g, Label = g, DisplayOrder = lookId });

            // Subjects
            foreach (var s in new[] { "None", "Bengali", "English", "History", "Islamic History & Culture", "Philosophy", "Islamic Studies", "Library Science", "Economics", "Political Science", "Sociology", "Social Work", "Anthropology", "Public Administration", "Physics", "Chemistry", "Mathematics", "Statistics", "Botany", "Zoology", "Geography & Environment", "Psychology", "Soil Science", "Accounting", "Management", "Marketing", "Finance & Banking", "Fine Arts", "Physical Education", "Business Administration", "Computer", "Civil", "Mechanical", "Electrical", "Medical", "Dentestry", "Engineering", "Law", "Pharma", "Agriculture", "Textile", "Lather", "Education" })
                lookupItems.Add(new LookupItem { Id = lookId++, Category = "AcademicSubject", Value = s, Label = s, DisplayOrder = lookId });

            // Professional Sectors
            foreach (var ps in new[] { "Ready-made Garments (RMG)", "Textiles & Spinning", "Pharmaceuticals", "Banking & Financial Services", "Information Technology (IT) & Software", "Telecommunications", "Agriculture & Crop Production", "Fisheries & Aquaculture", "Livestock & Poultry", "Agro-processing & Food Production", "Leather & Footwear", "Jute & Jute Goods", "Light Engineering", "Electronics & Electrical Appliances", "Real Estate & Housing", "Construction & Infrastructure", "Healthcare & Medical Services", "Education & Research", "Tourism & Hospitality", "Power, Energy & Mineral Resources", "Steel & Re-rolling", "Cement", "Ceramics", "Chemicals & Fertilizers", "Shipbuilding", "Transportation & Logistics", "Fast-Moving Consumer Goods (FMCG)", "Paper & Printing", "Plastic & Rubber Products", "Insurance", "Advertising & Media", "Legal & Consultancy Services", "Public Administration & Defense" })
                lookupItems.Add(new LookupItem { Id = lookId++, Category = "ProfessionalSector", Value = ps, Label = ps, DisplayOrder = lookId });

            modelBuilder.Entity<LookupItem>().HasData(lookupItems);

            // Seed Email Templates
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate 
                { 
                    Id = 1, 
                    Code = "OTP_EMAIL", 
                    Subject = "GHCAA Verification Code: {{OtpCode}}", 
                    Description = "Security code for login/registration",
                    Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Verification Code</h2><p>Hello <strong>{{FullName}}</strong>,</p><p>Your security code is:</p><div style='font-size: 24px; font-weight: bold; background: #f8f9fa; padding: 15px; text-align: center; border-radius: 5px; color: #3498db;'>{{OtpCode}}</div><p>Valid for 10 minutes. Do not share this code.</p></div>",
                    Variables = "['FullName', 'OtpCode']",
                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new EmailTemplate 
                { 
                    Id = 2, 
                    Code = "WELCOME_EMAIL", 
                    Subject = "Welcome to GHC Alumni Association!", 
                    Description = "Official induction message",
                    Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Welcome to GHCAA</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>Your membership has been approved! We are excited to have you as part of our community.</p><div style='background: #e8f4fd; padding: 15px; border-radius: 5px;'><p><strong>Membership No:</strong> {{MembershipNumber}}</p><p><strong>Default Password:</strong> <code style='background:#fff; padding:2px 5px;'>{{DefaultPassword}}</code></p></div><p>Please log in and change your password immediately.</p></div>",
                    Variables = "['FullName', 'MembershipNumber', 'DefaultPassword']",
                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new EmailTemplate 
                { 
                    Id = 3, 
                    Code = "FEE_REMINDER", 
                    Subject = "Annual Membership Subscription Due", 
                    Description = "Friendly reminder for yearly dues",
                    Body = "<div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'><h2 style='color: #2c3e50;'>Subscription Reminder</h2><p>Dear <strong>{{FullName}}</strong>,</p><p>This is a reminder that your annual membership subscription is now due.</p><p>Maintaining an active status ensures you continue to receive all alumni benefits and voting rights.</p><p>Thank you for your continued support!</p></div>",
                    Variables = "['FullName']",
                    LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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

            // Seed Members
            var shalin = new Member
            {
                Id = 1,
                FullName = "Habibur Rahman Shalin",
                FatherName = "Father",
                MotherName = "Mother",
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Gender = Domain.Enums.Gender.Male,
                BloodGroup = Domain.Enums.BloodGroup.APositive,
                NID = "0000000001",
                MobileNo = "01700000001",
                Email = "shalin.rahman@gmail.com",
                PresentAddress = "Munshiganj",
                PermanentAddress = "Munshiganj",
                EmergencyContactName = "Emergency",
                EmergencyContactRelation = "Family",
                EmergencyContactPhone = "01700000000",
                HSCAdmissionYear = 2013,
                HighestCertificate = "HSC",
                HighestCertificateGroup = "Science",
                HighestCertificateSubject = "None",
                HighestCertificatePassingYear = 2015,
                GHCAdmissionYear = 2013,
                GHCLastCertificate = "HSC",
                GHCLastCertificateGroup = "Science",
                GHCLastCertificateSubject = "None",
                GHCLastCertificatePassingYear = 2015,
                ProfessionalSector = "Engineering",
                Designation = "Software Engineer",
                Status = Domain.Enums.MembershipStatus.Active,
                AppliedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MembershipNumber = "GHC-2015-0001",
                MembershipType = Domain.Enums.MembershipType.Founding,
                LastUpdateDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ECPosition = Domain.Enums.ECPosition.President,
                EmailVerified = true,
                HasAcceptedTerms = true
            };

            var jane = new Member
            {
                Id = 101, // Use a distinct ID
                FullName = "Jane Doe",
                FatherName = "James Doe",
                MotherName = "Mary Doe",
                DateOfBirth = new DateTime(1992, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                Gender = Domain.Enums.Gender.Female,
                BloodGroup = Domain.Enums.BloodGroup.OPositive,
                NID = "0000000002",
                MobileNo = "01700000002",
                Email = "jane@example.com",
                PresentAddress = "Dhaka",
                PermanentAddress = "Dhaka",
                EmergencyContactName = "Friend",
                EmergencyContactRelation = "None",
                EmergencyContactPhone = "01700000003",
                HSCAdmissionYear = 2014,
                HighestCertificate = "HSC",
                HighestCertificateGroup = "Humanities",
                HighestCertificateSubject = "None",
                HighestCertificatePassingYear = 2016,
                GHCAdmissionYear = 2014,
                GHCLastCertificate = "HSC",
                GHCLastCertificateGroup = "Humanities",
                GHCLastCertificateSubject = "None",
                GHCLastCertificatePassingYear = 2016,
                ProfessionalSector = "Corporate",
                Designation = "Communications Manager",
                Status = Domain.Enums.MembershipStatus.Active,
                AppliedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MembershipNumber = "GHC-2016-0001",
                MembershipType = Domain.Enums.MembershipType.General,
                LastUpdateDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ECPosition = Domain.Enums.ECPosition.GeneralSecretary,
                EmailVerified = true,
                HasAcceptedTerms = true
            };

            modelBuilder.Entity<Member>().HasData(shalin, jane);

            // Seed Admin User: shalin
            // Password: shalin (hashed)
            var shalinUser = new User
            {
                Id = 2,
                Username = "shalin",
                PasswordHash = "$2a$11$J0UJbz.FdyElDw2mV22g1OikjTExwKvZ.c4eP3Wenc1MkmYDrgUme", // hardcoded "shalin"
                IsActive = true,
                IsArchived = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MemberId = 1 // Linked to shalin member
            };

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "superadmin",
                PasswordHash = "$2a$11$CQ4KnTDZ7qUQMNre86iruOpgOx8fEoMe2G3RF/1U4cCLa5ltYtE1O", // SuperAdminPassword123!
                IsActive = true,
                IsArchived = false,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MemberId = null 
            }, shalinUser);

            // Seed many-to-many Roles for Users
            modelBuilder.Entity("UserRoles").HasData(
                new { RolesId = 1, UsersId = 1 }, // SuperAdmin -> SuperAdmin Role
                new { RolesId = 2, UsersId = 2 }  // shalin -> Admin Role
            );

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

            // Seed Initial Membership Fees
            modelBuilder.Entity<MembershipFeeConfig>().HasData(
                new MembershipFeeConfig { Id = 1, MembershipType = Domain.Enums.MembershipType.Founding, Amount = 5000, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "Founding Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MembershipFeeConfig { Id = 2, MembershipType = Domain.Enums.MembershipType.Executive, Amount = 2000, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "Executive Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MembershipFeeConfig { Id = 3, MembershipType = Domain.Enums.MembershipType.General, Amount = 1000, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "General Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MembershipFeeConfig { Id = 4, MembershipType = Domain.Enums.MembershipType.Associate, Amount = 1000, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "Associate Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MembershipFeeConfig { Id = 5, MembershipType = Domain.Enums.MembershipType.Honorary, Amount = 0, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "Honorary Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MembershipFeeConfig { Id = 6, MembershipType = Domain.Enums.MembershipType.Advisory, Amount = 0, EffectiveDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), Description = "Advisory Member Fee", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed initial EC Period
            modelBuilder.Entity<ECPeriod>().HasData(
                new ECPeriod { Id = 1, Title = "Current Executive Committee", StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true }
            );

            // Link existing members to EC if applicable
            modelBuilder.Entity<ECMember>().HasData(
                new ECMember { Id = 1, ECPeriodId = 1, MemberId = 1, Position = Domain.Enums.ECPosition.President, StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new ECMember { Id = 2, ECPeriodId = 1, MemberId = 101, Position = Domain.Enums.ECPosition.GeneralSecretary, StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed News
            modelBuilder.Entity<NewsPost>().HasData(
                new NewsPost 
                { 
                    Id = 1, 
                    Title = "College Library Renovation Project Completed", 
                    Content = "The historic library of Govt. Haraganga College has been fully renovated with modern amenities and digital archiving systems, funded by the 1985 batch alumni.", 
                    Category = Domain.Enums.ArticleCategory.Regular, 
                    Status = Domain.Enums.SubmissionStatus.Approved,
                    IsActive = true, 
                    AuthorId = 2, 
                    PublishDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    ImageUrl = "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?q=80&w=2070"
                },
                new NewsPost 
                { 
                    Id = 2, 
                    Title = "Haragangian Global Meet 2026: London Chapter", 
                    Content = "GHCAA members in the UK gathered at the Royal Museum today to discuss international networking and scholarship opportunities for current students.", 
                    Category = Domain.Enums.ArticleCategory.Event,
                    Status = Domain.Enums.SubmissionStatus.Approved,
                    IsActive = true, 
                    AuthorId = 2, 
                    PublishDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc),
                    ImageUrl = "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?q=80&w=2070"
                }
            );

            // Seed Events
            modelBuilder.Entity<AlumniEvent>().HasData(
                new AlumniEvent 
                { 
                    Id = 1, 
                    Title = "Grand Reunion 2026", 
                    Description = "The biggest gathering of Haragangians across the globe. Join us for a day of nostalgia, networking, and cultural celebrations.", 
                    Date = new DateTime(2026, 5, 15, 9, 0, 0, DateTimeKind.Utc), 
                    Location = "College Ground, Munshiganj", 
                    RegistrationFee = 1500, 
                    IsActive = true, 
                    RegistrationDeadline = new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    ImageUrl = "https://images.unsplash.com/photo-1511578334221-d748ef50b502?q=80&w=2070"
                }
            );

            // Seed Jobs
            modelBuilder.Entity<JobOpportunity>().HasData(
                new JobOpportunity 
                { 
                    Id = 1, 
                    Title = "Senior Software Architect", 
                    Company = "GlobalTech Solutions", 
                    Location = "Dhaka, Bangladesh", 
                    Description = "Looking for an experienced architect to lead our fintech transition. Great benefits and remote flexibility.", 
                    Requirements = "10+ years of experience, C# Experts only.", 
                    ContactEmail = "careers@globaltech.com", 
                    Category = Domain.Enums.JobCategory.IT, 
                    PostedByMemberId = 1, 
                    PostedDate = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Utc),
                    ExpiryDate = new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc),
                    IsActive = true 
                }
            );

            // Seed Themes (Independence Day)
            modelBuilder.Entity<SpecialDayTheme>().HasData(
                new SpecialDayTheme 
                { 
                    Id = 1, 
                    Title = "Independence Day 2026", 
                    StartDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2026, 3, 30, 23, 59, 59, DateTimeKind.Utc),
                    BackgroundColor = "#d63031", // Deep Red
                    TextColor = "#ffffff",
                    AnnouncementText = "Happy 55th Independence Day! Celebrating our glorious history.",
                    IsEnabled = true
                }
            );

            // Seed Gallery
            modelBuilder.Entity<EventGallery>().HasData(
                new EventGallery 
                { 
                    Id = 1, 
                    Title = "Centennial Celebration", 
                    Description = "Highlights from the 100th-anniversary gala of Haraganga College.", 
                    EventDate = new DateTime(2025, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedByAdminId = 1,
                    IsActive = true,
                    IsFeatured = true
                },
                new EventGallery 
                { 
                    Id = 2, 
                    Title = "Campus Landscapes", 
                    Description = "Scenic views of the historic GHC campus buildings and grounds.", 
                    EventDate = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                    CreatedByAdminId = 1,
                    IsActive = true,
                    IsFeatured = true
                }
            );

            // Seed Photos
            modelBuilder.Entity<EventPhoto>().HasData(
                new EventPhoto { Id = 1, EventGalleryId = 1, PhotoPath = "https://images.unsplash.com/photo-1540575467063-178a50c2df87?q=80&w=2070", Caption = "Gala Evening" },
                new EventPhoto { Id = 2, EventGalleryId = 1, PhotoPath = "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?q=80&w=2069", Caption = "Alumni Networking" },
                new EventPhoto { Id = 3, EventGalleryId = 2, PhotoPath = "https://images.unsplash.com/photo-1562774053-701939374585?q=80&w=1986", Caption = "Main Administrative Building" },
                new EventPhoto { Id = 4, EventGalleryId = 2, PhotoPath = "https://images.unsplash.com/photo-1492538350424-aaee9f201774?q=80&w=2070", Caption = "College Playground" }
            );

            // Seed Academic Records
            modelBuilder.Entity<AcademicRecord>().HasData(
                new AcademicRecord 
                { 
                    Id = 1, MemberId = 1, InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", 
                    AdmissionYear = 2013, PassingYear = 2015, IsGHC = true 
                },
                new AcademicRecord 
                { 
                    Id = 2, MemberId = 101, InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Humanities", 
                    AdmissionYear = 2014, PassingYear = 2016, IsGHC = true 
                }
            );

            // Seed Professional Records
            modelBuilder.Entity<ProfessionalRecord>().HasData(
                new ProfessionalRecord 
                { 
                    Id = 1, MemberId = 1, OrganizationName = "GlobalTech Solutions", Designation = "Senior Software Architect", 
                    Sector = "Information Technology (IT) & Software", Location = "Dhaka", StartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                    IsCurrent = true 
                },
                new ProfessionalRecord 
                { 
                    Id = 2, MemberId = 101, OrganizationName = "Alumni Corp", Designation = "Communications Manager", 
                    Sector = "Advertising & Media", Location = "Dhaka", StartDate = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc), 
                    IsCurrent = true 
                }
            );
        }
    }
}