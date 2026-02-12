using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private ApplicationDbContext _context = null!;
        private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
        private Mock<ILogger<UserService>> _mockLogger = null!;
        private UserService _service = null!;

        [SetUp]
        public void Setup()
        {
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _mockLogger = new Mock<ILogger<UserService>>();
            _service = new UserService(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        [Test]
        public async Task CreateUserAccountAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var member = new Member { FullName = "Test Member", Email = "valid@e.com", NID = "V1", MobileNo = "V1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var memberId = member.Id;
            var username = "GHC-2007-0001";
            var password = "TestPassword123";

            // Act
            var user = await _service.CreateUserAccountAsync(memberId, username, password);

            // Assert
            user.Should().NotBeNull();
            user.Username.Should().Be(username);
            user.MemberId.Should().Be(memberId);
            user.IsActive.Should().BeTrue();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().NotBe(password); // Should be hashed
            
            // Verify password hash is BCrypt
            BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateMemberId_ShouldThrowException()
        {
            // Arrange
            var member = new Member { FullName = "Test Member", Email = "dup@e.com", NID = "D1", MobileNo = "D1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var memberId = member.Id;
            await _context.Users.AddAsync(new User
            {
                Username = "existing",
                PasswordHash = "hash",
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var act = async () => await _service.CreateUserAccountAsync(memberId, "newuser", "password");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User account already exists for member {memberId}");
        }

        [Test]
        public async Task CreateUserAccountAsync_WithDuplicateUsername_ShouldThrowException()
        {
            // Arrange
            var member1 = new Member { FullName = "Test Member 1", Email = "test1@e.com", NID = "1231", MobileNo = "1231", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            var member2 = new Member { FullName = "Test Member 2", Email = "test2@e.com", NID = "1232", MobileNo = "1232", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddRangeAsync(member1, member2);
            await _context.SaveChangesAsync();

            var username = "GHC-2007-0001";
            await _context.Users.AddAsync(new User
            {
                Username = username,
                PasswordHash = "hash",
                MemberId = null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var act = async () => await _service.CreateUserAccountAsync(member2.Id, username, "password");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Username '{username}' is already taken");
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturn8CharacterPassword()
        {
            // Act
            var password = _service.GenerateDefaultPassword();

            // Assert
            password.Should().NotBeNullOrEmpty();
            password.Length.Should().Be(8);
            password.Should().MatchRegex("^[A-Za-z0-9]{8}$"); // Only alphanumeric
        }

        [Test]
        public void GenerateDefaultPassword_ShouldReturnDifferentPasswords()
        {
            // Act
            var password1 = _service.GenerateDefaultPassword();
            var password2 = _service.GenerateDefaultPassword();
            var password3 = _service.GenerateDefaultPassword();

            // Assert - At least one should be different (very high probability)
            var allSame = password1 == password2 && password2 == password3;
            allSame.Should().BeFalse();
        }

        [Test]
        public async Task CreateUserAccountAsync_ShouldHashPasswordWithBCrypt()
        {
            // Arrange
            var member = new Member { FullName = "BCrypt Test", Email = "bcrypt@e.com", NID = "B1", MobileNo = "B1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var password = "MySecurePassword123";

            // Act
            var user = await _service.CreateUserAccountAsync(member.Id, "testuserhash", password);

            // Assert
            user.PasswordHash.Should().StartWith("$2"); // BCrypt hash starts with $2a, $2b, etc.
            BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("WrongPassword", user.PasswordHash).Should().BeFalse();
        }

        [Test]
        public async Task ChangePasswordAsync_WithValidData_ShouldUpdatePassword()
        {
            // Arrange
            var member = new Member { FullName = "Test Member", Email = "test@e.com", NID = "123", MobileNo = "123", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "user1", "OldPassword123");
            
            // Act
            var result = await _service.ChangePasswordAsync(user.Id, "OldPassword123", "NewPassword456");

            // Assert
            result.Should().BeTrue();
            var updatedUser = await _context.Users.FindAsync(user.Id);
            BCrypt.Net.BCrypt.Verify("NewPassword456", updatedUser!.PasswordHash).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify("OldPassword123", updatedUser.PasswordHash).Should().BeFalse();
        }

        [Test]
        public async Task ChangePasswordAsync_WithWrongOldPassword_ShouldReturnFalse()
        {
            // Arrange
            var member = new Member { FullName = "Test Member", Email = "pw2@e.com", NID = "P2", MobileNo = "P2", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var user = await _service.CreateUserAccountAsync(member.Id, "user1", "OldPassword123");

            // Act
            var result = await _service.ChangePasswordAsync(user.Id, "WrongOldPassword", "NewPassword456");


            // Assert
            result.Should().BeFalse();
            var updatedUser = await _context.Users.FindAsync(user.Id);
            BCrypt.Net.BCrypt.Verify("OldPassword123", updatedUser!.PasswordHash).Should().BeTrue();
        }
    }
}
