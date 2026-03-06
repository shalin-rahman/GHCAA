using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using System.Linq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NetworkingServiceTests : TestBase
{
    private NetworkingService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new NetworkingService(_context);

        // Clear seeded EC data for deterministic assertions
        _context.ECMembers.RemoveRange(_context.ECMembers);
        _context.ECPeriods.RemoveRange(_context.ECPeriods);
        _context.SaveChanges();
    }

    [Test]
    public async Task SearchMembersAsync_ShouldHonorPrivacyFlags()
    {
        var publicMember = CreateValidMember("Public Jane", "public@nttest.com", "01700001111", "NTST1111");
        publicMember.IsEmailPublic = true;
        publicMember.IsMobilePublic = true;
        publicMember.IsAddressPublic = true;

        var privateMember = CreateValidMember("Private John", "private@nttest.com", "01711112222", "NTST2222");
        privateMember.IsEmailPublic = false;
        privateMember.IsMobilePublic = false;
        privateMember.IsAddressPublic = false;

        _context.Members.AddRange(publicMember, privateMember);
        await _context.SaveChangesAsync();

        var results = (await _service.SearchMembersAsync(new MemberSearchFilterDto())).ToList();

        var publicResult = results.First(r => r.FullName == "Public Jane");
        publicResult.Email.Should().Be("public@nttest.com");
        publicResult.MobileNo.Should().Be("01700001111");

        var privateResult = results.First(r => r.FullName == "Private John");
        privateResult.Email.Should().Be("Confidential");
        privateResult.MobileNo.Should().Be("Confidential");
        privateResult.PresentAddress.Should().Be("Confidential");
    }

    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldReturnMembersWithECPosition()
    {
        var president = CreateValidMember("EC President", "ep@nttest.com", "01722223333", "NTST3333");
        president.ECPosition = Enums.ECPosition.President;

        var normal = CreateValidMember("Normal Member", "nm@nttest.com", "01733334444", "NTST4444");
        normal.ECPosition = Enums.ECPosition.None;

        _context.Members.AddRange(president, normal);
        await _context.SaveChangesAsync();

        var period = new ECPeriod { Title = "Test Period", StartDate = DateTime.UtcNow, IsActive = true };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();

        _context.ECMembers.Add(new ECMember { MemberId = president.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var results = await _service.GetExecutiveCommitteeAsync();

        results.Should().HaveCount(1);
        results.First().FullName.Should().Be("EC President");
    }

    [Test]
    public async Task SearchMembersAsync_ShouldHideInactiveAndArchivedMembers()
    {
        var inactive = CreateValidMember("Inactive Member", "i@nttest.com", "01744445555", "NTST5555");
        inactive.Status = Enums.MembershipStatus.InactivePayment;

        var archived = CreateValidMember("Archived Member", "a@nttest.com", "01755556666", "NTST6666");
        archived.IsArchived = true;

        var active = CreateValidMember("Active Member", "active@nttest.com", "01766667777", "NTST7777");
        active.Status = Enums.MembershipStatus.Active;

        _context.Members.AddRange(inactive, archived, active);
        await _context.SaveChangesAsync();

        var results = await _service.SearchMembersAsync(new MemberSearchFilterDto());

        results.Should().Contain(r => r.FullName == "Active Member");
        results.Should().NotContain(r => r.FullName == "Inactive Member");
        results.Should().NotContain(r => r.FullName == "Archived Member");
    }

    private Member CreateValidMember(string name, string email, string phone, string nid)
    {
        return new Member
        {
            FullName = name, Email = email, MobileNo = phone, NID = nid,
            FatherName = "Father", MotherName = "Mother",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Enums.Gender.Male, BloodGroup = Enums.BloodGroup.APositive,
            PresentAddress = "Present", PermanentAddress = "Permanent",
            EmergencyContactName = "Emergency", EmergencyContactRelation = "Relation", EmergencyContactPhone = "01111111111",
            HSCAdmissionYear = 2005, GHCAdmissionYear = 2005, LastCertificateFromGHC = "HSC",
            SubjectGroup = "Science", GHCLastCertificatePassingYear = 2007,
            ProfessionalSector = "IT", Designation = "Software Engineer",
            Status = Enums.MembershipStatus.Active, AppliedDate = DateTime.UtcNow
        };
    }
}
