using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;

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

    [Category("FR-03")]
    [Category("FR-05")]
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

    [Category("FR-05")]
    [Test]
    public async Task SearchPublicDirectoryAsync_ReturnsNamePhotoAndBatchOnly()
    {
        var member = await CreateAndSaveTestMemberAsync("Public Listing", "listing.nt@example.com", "01155556666", "NTST5555");
        member.Status = Enums.MembershipStatus.Active;
        member.IsEmailPublic = true;
        member.IsMobilePublic = true;
        member.BloodGroup = Enums.BloodGroup.APositive;
        member.PhotoPath = "/uploads/photos/listing.jpg";
        member.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Honours", Subject = "Physics", PassingYear = 2008, IsOrgProfile = true } };
        await _context.SaveChangesAsync();

        var results = (await _service.SearchPublicDirectoryAsync(new MemberSearchFilterDto { Query = "Public Listing", PageSize = 100 })).Items.ToList();

        var result = results.Single();
        result.Id.Should().Be(member.Id);
        result.FullName.Should().Be("Public Listing");
        result.PhotoPath.Should().Be("/uploads/photos/listing.jpg");
        result.PassingYear.Should().Be(2008);
        result.Degree.Should().BeNull();
        result.Subject.Should().BeNull();
        result.Email.Should().BeNull();
        result.MobileNo.Should().BeNull();
        result.MembershipNumber.Should().BeNull();
        result.BloodGroup.Should().Be(Enums.BloodGroup.Unknown);
        result.ECHistory.Should().BeEmpty();
    }

    [Category("FR-05")]
    [Test]
    public async Task SearchPublicDirectoryAsync_IgnoresFiltersOtherThanNameAndBatch()
    {
        var member = await CreateAndSaveTestMemberAsync("Filter Probe", "probe.nt@example.com", "01177778888", "NTST7777");
        member.Status = Enums.MembershipStatus.Active;
        member.IsEmailPublic = true;
        member.BloodGroup = Enums.BloodGroup.APositive;
        await _context.SaveChangesAsync();

        // A blood group filter that would exclude the member is dropped, so the member is still listed.
        var byBlood = await _service.SearchPublicDirectoryAsync(new MemberSearchFilterDto { Query = "Filter Probe", BloodGroup = "ONegative", PageSize = 100 });
        byBlood.Items.Should().ContainSingle();

        // Email and membership number do not match in the public listing.
        (await _service.SearchPublicDirectoryAsync(new MemberSearchFilterDto { Query = "probe.nt@example.com" })).Items.Should().BeEmpty();
        (await _service.SearchPublicDirectoryAsync(new MemberSearchFilterDto { Query = "NTST7777" })).Items.Should().BeEmpty();
    }

    [Category("FR-34")]
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

        _context.ECMembers.Add(new ECMember { MemberId = president.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var results = await _service.GetExecutiveCommitteeAsync();

        results.Should().HaveCount(1);
        results.First().FullName.Should().Be("EC President");
    }

    [Category("FR-05")]
    [Test]
    public async Task SearchMembersAsync_WithNewTableFilters_ShouldReturnCorrectMembers()
    {
        // Setup scenarios: 
        var jane = await CreateAndSaveTestMemberAsync("Jane Doe", "jane.nt@example.com", "01100000091", "1234567891");
        jane.Status = Enums.MembershipStatus.Active;
        jane.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 1938, IsOrgProfile = true } };
        jane.ProfessionalHistory = new List<ProfessionalRecord> { new ProfessionalRecord { OrganizationName = "TechCo", Designation = "Developer", Sector = "IT", IsCurrent = true, StartDate = DateTime.UtcNow.AddYears(-1) } };

        var john = await CreateAndSaveTestMemberAsync("John Smith", "john.nt@example.com", "01100000092", "1234567892");
        john.Status = Enums.MembershipStatus.Active;
        john.AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Hons", Subject = "Business", PassingYear = 1940, IsOrgProfile = true } };
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

    [Category("FR-34")]
    [Test]
    public async Task GetExecutiveCommitteeAsync_ShouldPopulateBatchInformation()
    {
        // 30.27: EC/networking summary cards must show PassingYear/Degree/Subject -
        // regression test for the "Executive Committee batch information is missing" bug.
        var president = await CreateAndSaveTestMemberAsync("EC Batch President", "ecbatch.nt@example.com", "01100009999", "NTST9999");
        president.Status = Enums.MembershipStatus.Active;
        president.AcademicHistory = new List<AcademicRecord>
        {
            new AcademicRecord { InstitutionName = "Govt. Haraganga College", Degree = "Honours", Subject = "Political Science", PassingYear = 2005, IsOrgProfile = true }
        };

        var period = new ECPeriod { Title = "Batch Test Period", StartDate = DateTime.UtcNow, IsActive = true };
        _context.ECPeriods.Add(period);
        await _context.SaveChangesAsync();

        _context.ECMembers.Add(new ECMember { MemberId = president.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var results = await _service.GetExecutiveCommitteeAsync();

        var summary = results.First(r => r.FullName == "EC Batch President");
        summary.PassingYear.Should().Be(2005);
        summary.Degree.Should().Be("Honours");
        summary.Subject.Should().Be("Political Science");
    }

    [Category("FR-34")]
    [Test]
    public async Task SearchMembersAsync_ECPositionFilter_IgnoresAPeriodThatIsNotActive()
    {
        // 37.1u. Winners are written into the next period on Declare, before it is made active.
        var sitting = await CreateAndSaveTestMemberAsync("Sitting President", "sitting.nt@example.com", "01155556666", "NTST5556");
        sitting.Status = Enums.MembershipStatus.Active;
        var elected = await CreateAndSaveTestMemberAsync("Elected President", "elected.nt@example.com", "01155557777", "NTST5557");
        elected.Status = Enums.MembershipStatus.Active;
        var current = new ECPeriod { Title = "Current", StartDate = DateTime.UtcNow.AddYears(-1), IsActive = true };
        var next = new ECPeriod { Title = "Next", StartDate = DateTime.UtcNow.AddDays(30) };
        _context.ECPeriods.AddRange(current, next);
        _context.ECMembers.Add(new ECMember { MemberId = sitting.Id, ECPeriod = current, Position = EcPositionIds.President, StartDate = current.StartDate });
        _context.ECMembers.Add(new ECMember { MemberId = elected.Id, ECPeriod = next, Position = EcPositionIds.President, StartDate = next.StartDate });
        await _context.SaveChangesAsync();

        var results = await _service.SearchMembersAsync(new MemberSearchFilterDto { ECPosition = "President", PageSize = 100 });

        results.Items.Select(x => x.FullName).Should().Equal("Sitting President");
        (await _service.GetExecutiveCommitteeAsync()).Select(x => x.FullName).Should().Contain("Sitting President").And.NotContain("Elected President");
    }

    [Category("FR-34")]
    [Test]
    public async Task SearchMembersAsync_ECPositionFilter_MatchesAnAdminAddedPositionKey_IgnoringCase()
    {
        // 95.2: positions an admin adds have no enum value, so the filter has to match on the key.
        var adviser = await CreateAndSaveTestMemberAsync("Sitting Adviser", "adviser.nt@example.com", "01155558888", "NTST5558");
        adviser.Status = Enums.MembershipStatus.Active;
        var treasurer = await CreateAndSaveTestMemberAsync("Sitting Treasurer", "treasurer.nt@example.com", "01155559999", "NTST5559");
        treasurer.Status = Enums.MembershipStatus.Active;
        var position = new EcPosition { Key = "SeniorAdviser", Name = "Senior Adviser", DisplayOrder = 30 };
        var period = new ECPeriod { Title = "Current", StartDate = DateTime.UtcNow.AddYears(-1), IsActive = true };
        _context.EcPositions.Add(position);
        _context.ECPeriods.Add(period);
        _context.ECMembers.Add(new ECMember { MemberId = adviser.Id, ECPeriod = period, EcPosition = position, StartDate = period.StartDate });
        _context.ECMembers.Add(new ECMember { MemberId = treasurer.Id, ECPeriod = period, Position = EcPositionIds.Treasurer, StartDate = period.StartDate });
        await _context.SaveChangesAsync();

        var results = await _service.SearchMembersAsync(new MemberSearchFilterDto { ECPosition = " senioradviser ", PageSize = 100 });

        results.Items.Select(x => x.FullName).Should().Equal("Sitting Adviser");
    }

    [Category("FR-05")]
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

    [Category("FR-05")]
    [Test]
    public async Task SearchMembersAsync_CursorPagination_ShouldPageThroughAllMembersOnce()
    {
        for (var i = 0; i < 5; i++)
        {
            var member = await CreateAndSaveTestMemberAsync($"Cursor Member {i:D2}", $"cursor{i}.nt@example.com", $"017{i:D8}", $"NTSTCUR{i:D3}");
            member.Status = Enums.MembershipStatus.Active;
        }
        await _context.SaveChangesAsync();

        var seen = new List<string>();
        string? cursor = null;
        var pagesFetched = 0;

        do
        {
            var page = await _service.SearchMembersAsync(new MemberSearchFilterDto
            {
                Query = "Cursor Member",
                PageSize = 2,
                Cursor = cursor
            });

            seen.AddRange(page.Items.Select(i => i.FullName));
            cursor = page.NextCursor;
            pagesFetched++;
        } while (cursor != null && pagesFetched < 10);

        seen.Should().HaveCount(5);
        seen.Distinct().Should().HaveCount(5, "cursor pagination must not repeat or skip a member across pages");
        seen.Should().BeInAscendingOrder();
    }

    [Category("FR-05")]
    [Test]
    public async Task SearchMembersAsync_CursorPagination_EmptyResult_ShouldReturnNoItemsAndNoNextCursor()
    {
        var result = await _service.SearchMembersAsync(new MemberSearchFilterDto
        {
            Query = "No Member Matches This Query",
            PageSize = 20
        });

        result.Items.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Category("FR-05")]
    [Test]
    public async Task SearchMembersAsync_InvalidCursor_ShouldFallBackToFirstPageInsteadOfThrowing()
    {
        var alpha = await CreateAndSaveTestMemberAsync("Bad Cursor Alpha", "bc.alpha.nt@example.com", "01100003001", "NTSTBC001");
        alpha.Status = Enums.MembershipStatus.Active;
        var beta = await CreateAndSaveTestMemberAsync("Bad Cursor Beta", "bc.beta.nt@example.com", "01100003002", "NTSTBC002");
        beta.Status = Enums.MembershipStatus.Active;
        await _context.SaveChangesAsync();

        var result = await _service.SearchMembersAsync(new MemberSearchFilterDto
        {
            Query = "Bad Cursor",
            PageSize = 20,
            Cursor = "not-a-valid-base64-cursor!!"
        });

        result.Items.Should().HaveCount(2, "a stale/corrupt cursor should degrade to a first page, not fail the request");
        result.Items.Select(i => i.FullName).Should().Contain(new[] { "Bad Cursor Alpha", "Bad Cursor Beta" });
    }
}
