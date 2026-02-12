using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NetworkingServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private NetworkingService _service = null!;

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

        _service = new NetworkingService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task SearchMembersAsync_ShouldHonorPrivacyFlags()
    {
        // Arrange
        var publicMember = CreateValidMember("Public Jane", "public@example.com", "01700000000", "1111111111");
        publicMember.IsEmailPublic = true;
        publicMember.IsMobilePublic = true;
        publicMember.IsAddressPublic = true;

        var privateMember = CreateValidMember("Private John", "private@example.com", "01711111111", "2222222222");
        privateMember.IsEmailPublic = false;
        privateMember.IsMobilePublic = false;
        privateMember.IsAddressPublic = false;

        _context.Members.AddRange(publicMember, privateMember);
        await _context.SaveChangesAsync();

        // Act
        var results = (await _service.SearchMembersAsync(new MemberSearchFilterDto())).ToList();

        // Assert
        var publicResult = results.First(r => r.FullName == "Public Jane");
        publicResult.Email.Should().Be("public@example.com");
        publicResult.MobileNo.Should().Be("01700000000");

        var privateResult = results.First(r => r.FullName == "Private John");
        privateResult.Email.Should().Be("Confidential");
        privateResult.MobileNo.Should().Be("Confidential");
        privateResult.PresentAddress.Should().Be("Confidential");
    }

    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldReturnMembersWithECPosition()
    {
        // Arrange
        var president = CreateValidMember("President", "p@e.com", "01722222222", "3333333333");
        president.ECPosition = Enums.ECPosition.President;
        
        var normal = CreateValidMember("Normal Member", "m@e.com", "01733333333", "4444444444");
        normal.ECPosition = Enums.ECPosition.None;

        _context.Members.AddRange(president, normal);
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.GetExecutiveCommitteeAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().FullName.Should().Be("President");
    }

    [Test]
    public async Task SearchMembersAsync_ShouldHideInactiveAndArchivedMembers()
    {
        // Arrange
        var inactive = CreateValidMember("Inactive Member", "i@e.com", "01744444444", "5555555555");
        inactive.Status = Enums.MembershipStatus.InactivePayment;

        var archived = CreateValidMember("Archived Member", "a@e.com", "01755555555", "6666666666");
        archived.IsArchived = true;

        var active = CreateValidMember("Active Member", "active@e.com", "01766666666", "7777777777");
        active.Status = Enums.MembershipStatus.Active;

        _context.Members.AddRange(inactive, archived, active);
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.SearchMembersAsync(new MemberSearchFilterDto());

        // Assert
        results.Should().HaveCount(1);
        results.First().FullName.Should().Be("Active Member");
    }

    private Member CreateValidMember(string name, string email, string phone, string nid)
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
            PresentAddress = "Present",
            PermanentAddress = "Permanent",
            EmergencyContactName = "Emergency",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01111111111",
            HSCAdmissionYear = 2005,
            GHCAdmissionYear = 2005,
            LastCertificateFromGHC = "HSC",
            SubjectGroup = "Science",
            GHCLastCertificatePassingYear = 2007,
            ProfessionalSector = "IT",
            Designation = "Software Engineer",
            Status = Enums.MembershipStatus.Active,
            AppliedDate = DateTime.UtcNow
        };
    }
}
