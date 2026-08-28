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
        var publicMember = await CreateAndSaveTestMemberAsync("Alpha Jane", "public.nt@example.com", "01100001111", "NTST1111");
        publicMember.Status = Enums.MembershipStatus.Active;
        publicMember.IsEmailPublic = true;
        publicMember.IsMobilePublic = true;
        publicMember.IsAddressPublic = true;
        await _context.SaveChangesAsync();

        var privateMember = await CreateAndSaveTestMemberAsync("Alpha John", "private.nt@example.com", "01111112222", "NTST2222");
        privateMember.Status = Enums.MembershipStatus.Active;
        privateMember.IsEmailPublic = false;
        privateMember.IsMobilePublic = false;
        privateMember.IsAddressPublic = false;
        await _context.SaveChangesAsync();

        var filter = new MemberSearchFilterDto { PageSize = 100 };
        var results = (await _service.SearchMembersAsync(filter)).Items.ToList();

        var publicResult = results.First(r => r.FullName == "Alpha Jane");
        publicResult.Email.Should().Be("public.nt@example.com");
        publicResult.MobileNo.Should().Be("01100001111");

        var privateResult = results.First(r => r.FullName == "Alpha John");
        privateResult.Email.Should().Be("Confidential");
        privateResult.MobileNo.Should().Be("Confidential");
    }

    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldReturnMembersWithECPosition()
    {
        var president = await CreateAndSaveTestMemberAsync("EC President", "ep.nt@example.com", "01122223333", "NTST3333");
        president.Status = Enums.MembershipStatus.Active;
        var normal = await CreateAndSaveTestMemberAsync("Normal Member", "nm.nt@example.com", "01133334444", "NTST4444");
        normal.Status = Enums.MembershipStatus.Active;

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
        var jane = await CreateAndSaveTestMemberAsync("Jane Doe", "jane.nt@example.com", "01100000091", "1234567891");
        jane.Status = Enums.MembershipStatus.Active;
        jane.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 1938, IsGHC = true } };
        jane.ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "TechCo", Designation = "Developer", Sector = "IT", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-1) } };

        var john = await CreateAndSaveTestMemberAsync("John Smith", "john.nt@example.com", "01100000092", "1234567892");
        john.Status = Enums.MembershipStatus.Active;
        john.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Hons", Subject = "Business", PassingYear = 1940, IsGHC = true } };
        john.ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "BankCorp", Designation = "Manager", Sector = "Banking-NT-Test", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-2) } };

        await _context.SaveChangesAsync();

        // 1. Test PassingYear filter (1938 - unique in seed)
        var res1 = await _service.SearchMembersAsync(new MemberSearchFilterDto { PassingYear = 1938, PageSize = 100 });
        res1.Items.Should().HaveCount(1);
        res1.Items.First().FullName.Should().Be("Jane Doe");

        // 2. Test ProfessionalSector filter — a plain "Banking" value collided with real alumni
        // records added to the seed (several actual bankers), so this uses a distinctive marker
        // that can't collide with genuine seed data instead of relying on "currently unique in seed".
        var res2 = await _service.SearchMembersAsync(new MemberSearchFilterDto { ProfessionalSector = "Banking-NT-Test", PageSize = 100 });
        res2.Items.Should().HaveCount(1);
        res2.Items.First().FullName.Should().Be("John Smith");

        // 3. Test Designation filter (Developer)
        var res3 = await _service.SearchMembersAsync(new MemberSearchFilterDto { Designation = "Developer", PageSize = 100 });
        res3.Items.Should().HaveCount(1);
        res3.Items.First().FullName.Should().Be("Jane Doe");
    }

    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldPopulateBatchInformation()
    {
        // 30.27: EC/networking summary cards must show PassingYear/Degree/Subject -
        // regression test for the "Executive Committee batch information is missing" bug.
        var president = await CreateAndSaveTestMemberAsync("EC Batch President", "ecbatch.nt@example.com", "01100009999", "NTST9999");
        president.Status = Enums.MembershipStatus.Active;
        president.AcademicHistory = new List<AcademicRecord>
        {
            new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Honours", Subject = "Political Science", PassingYear = 2005, IsGHC = true }
        };

        var period = new ECPeriod { Title = "Batch Test Period", StartDate = DateTime.UtcNow, IsActive = true };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();

        _context.ECMembers.Add(new ECMember { MemberId = president.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var results = await _service.GetExecutiveCommitteeAsync();

        var summary = results.First(r => r.FullName == "EC Batch President");
        summary.PassingYear.Should().Be(2005);
        summary.Degree.Should().Be("Honours");
        summary.Subject.Should().Be("Political Science");
    }

    [Test]
    public async Task SearchMembersAsync_ShouldHideInactiveAndArchivedMembers()
    {
        var inactive = await CreateAndSaveTestMemberAsync("Inactive Member", "i.nt@example.com", "01144445555", "NTST5555");
        inactive.Status = Enums.MembershipStatus.InactivePayment;

        var archived = await CreateAndSaveTestMemberAsync("Archived Member", "a.nt@example.com", "01155556666", "NTST6666");
        archived.IsArchived = true;

        var active = await CreateAndSaveTestMemberAsync("Active Member", "active.nt@example.com", "01166667777", "NTST7777");
        active.Status = Enums.MembershipStatus.Active;

        await _context.SaveChangesAsync();

        var result = await _service.SearchMembersAsync(new MemberSearchFilterDto { PageSize = 1000 });

        result.Items.Should().Contain(r => r.FullName == "Active Member");
        result.Items.Should().NotContain(r => r.FullName == "Inactive Member");
        result.Items.Should().NotContain(r => r.FullName == "Archived Member");
    }
}
