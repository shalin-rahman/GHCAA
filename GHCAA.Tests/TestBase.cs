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

        protected Member CreateTestMember(string name = "John Doe", string email = "test@example.com", string phone = "01712345678", string nid = "1234567890")
        {
            return new Member
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
                HasAcceptedTerms = true
            };
        }
    }
}
