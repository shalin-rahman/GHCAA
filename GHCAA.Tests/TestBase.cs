using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GHCAA.Tests
{
    public class TestBase
    {
        protected ApplicationDbContext _context = null!;
        private SqliteConnection _connection = null!;

        [SetUp]
        public void BaseSetup()
        {
            ApplicationDbContext.IsSeedDisabled = false;
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        [TearDown]
        public void BaseTearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        protected void DetachAll()
        {
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries) entry.State = EntityState.Detached;
        }

        protected async Task<Member> CreateAndSaveTestMemberAsync(string name = "John Doe", string email = "test@example.com", string phone = "01712345678", string nid = "1234567890", int passingYear = 2020)
        {
            var member = new Member
            {
                FullName = name,
                Email = email,
                MobileNo = phone,
                NID = nid,
                FatherName = "Father",
                MotherName = "Mother",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = Enums.Gender.Male,
                BloodGroup = Enums.BloodGroup.APositive,
                PresentAddress = "Present Address",
                PermanentAddress = "Permanent Address",
                EmergencyContactName = "Emergency",
                EmergencyContactRelation = "Relation",
                EmergencyContactPhone = "01812345678",
                Status = Enums.MembershipStatus.Active,
                AppliedDate = DateTime.UtcNow,
                HasAcceptedTerms = true,
                AcademicHistory = new List<AcademicRecord>
                {
                    new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = passingYear, IsGHC = true }
                }
            };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        protected async Task<AlumniEvent> CreateAndSaveTestEventAsync(string title = "Test Event", decimal fee = 100)
        {
            var ev = new AlumniEvent
            {
                Title = title,
                Description = "Test Description",
                Date = DateTime.UtcNow.AddDays(30),
                Location = "Test Location",
                RegistrationFee = fee,
                IsActive = true
            };
            await _context.AlumniEvents.AddAsync(ev);
            await _context.SaveChangesAsync();
            return ev;
        }

        protected async Task<User> CreateAndSaveTestUserAsync(int memberId, string username, string password = "TestPassword123")
        {
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
