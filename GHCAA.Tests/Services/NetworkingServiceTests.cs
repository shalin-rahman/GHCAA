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

        var results = (await _service.SearchMembersAsync(new MemberSearchFilterDto())).Items.ToList();

        var publicResult = results.First(r => r.FullName == "Public Jane");
        publicResult.Email.Should().Be("public@nttest.com");
        publicResult.MobileNo.Should().Be("01700001111");

        var privateResult = results.First(r => r.FullName == "Private John");
        privateResult.Email.Should().Be("Confidential");
        privateResult.MobileNo.Should().Be("Confidential");
    }

    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldReturnMembersWithECPosition()
    {
        var president = CreateValidMember("EC President", "ep@nttest.com", "01722223333", "NTST3333");

        var normal = CreateValidMember("Normal Member", "nm@nttest.com", "01733334444", "NTST4444");

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
    public async Task SearchMembersAsync_WithNewTableFilters_ShouldReturnCorrectMembers()
    {
        // Setup scenarios: 
        // 1. Jane: GHC 2005, IT Sector, Developer
        // 2. John: GHC 2010, Banking, Manager
        var jane = CreateValidMember("Jane Doe", "jane@nt.com", "01700000001", "1234567801");
        jane.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true } };
        jane.ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "TechCo", Designation = "Developer", Sector = "IT", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-1) } };

        var john = CreateValidMember("John Smith", "john@nt.com", "01700000002", "1234567802");
        john.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Hons", Subject = "Business", PassingYear = 2010, IsGHC = true } };
        john.ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "BankCorp", Designation = "Manager", Sector = "Banking", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-2) } };

        _context.Members.AddRange(jane, john);
        await _context.SaveChangesAsync();

        // 1. Test PassingYear filter (2005)
        var res1 = await _service.SearchMembersAsync(new MemberSearchFilterDto { PassingYear = 2005 });
        res1.Items.Should().HaveCount(1);
        res1.Items.First().FullName.Should().Be("Jane Doe");

        // 2. Test ProfessionalSector filter (Banking)
        var res2 = await _service.SearchMembersAsync(new MemberSearchFilterDto { ProfessionalSector = "Banking" });
        res2.Items.Should().HaveCount(1);
        res2.Items.First().FullName.Should().Be("John Smith");

        // 3. Test Designation filter (Developer)
        var res3 = await _service.SearchMembersAsync(new MemberSearchFilterDto { Designation = "Developer" });
        res3.Items.Should().HaveCount(1);
        res3.Items.First().FullName.Should().Be("Jane Doe");
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

        var result = await _service.SearchMembersAsync(new MemberSearchFilterDto());

        result.Items.Should().Contain(r => r.FullName == "Active Member");
        result.Items.Should().NotContain(r => r.FullName == "Inactive Member");
        result.Items.Should().NotContain(r => r.FullName == "Archived Member");
    }

    private Member CreateValidMember(string name, string email, string phone, string nid)
    {
        return CreateTestMember(name, email, phone, nid);
    }
}
