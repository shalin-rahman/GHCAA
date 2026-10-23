using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Moq;

namespace GHCAA.Tests.Services;

// TODO 84.12. The assistant narrows a member search by passing year and sector, on the same
// base set as the directory.
[TestFixture]
[Category("FR-42")]
public class AssistantServiceTests : TestBase
{
    private AssistantService _service = null!;

    [SetUp]
    public void Setup()
    {
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        _service = new AssistantService(_context, orgConfig.Object);
    }

    private async Task<Member> AddAsync(string name, string email, string phone, string nid, int year, string? sector)
    {
        var member = await CreateAndSaveTestMemberAsync(name, email, phone, nid, year);
        if (sector != null)
        {
            member.ProfessionalHistory.Add(new ProfessionalRecord
            {
                OrganizationName = "Org",
                Designation = "Officer",
                Sector = sector,
                StartDate = new DateTime(2000, 1, 1),
                IsCurrent = true,
            });
            await _context.SaveChangesAsync();
        }
        return member;
    }

    [Test]
    public async Task AskAsync_YearInQuery_KeepsOnlyThatPassingYear()
    {
        await AddAsync("Asst Year Match", "a1@example.com", "01700000001", "AS0001", 1961, null);
        await AddAsync("Asst Year Other", "a2@example.com", "01700000002", "AS0002", 1962, null);

        var result = await _service.AskAsync("Find members from 1961");

        var names = result.FoundMembers!.Select(m => m.FullName).ToList();
        names.Should().Contain("Asst Year Match").And.NotContain("Asst Year Other");
        result.FoundMembers!.Should().OnlyContain(m => m.GHCLastCertificatePassingYear == 1961);
    }

    [Test]
    public async Task AskAsync_SectorWord_MatchesCurrentJobSector()
    {
        await AddAsync("Asst Banker", "b1@example.com", "01700000003", "AS0003", 1963, "Banking & Financial Services");
        await AddAsync("Asst Doctor", "b2@example.com", "01700000004", "AS0004", 1963, "Healthcare & Medical Services");

        var result = await _service.AskAsync("who works in a bank from 1963");

        var found = result.FoundMembers!.Single();
        found.FullName.Should().Be("Asst Banker");
        found.ProfessionalSector.Should().Be("Banking & Financial Services");
        found.Designation.Should().Be("Officer");
    }

    [Test]
    public async Task AskAsync_LeavesOutInactiveAndArchivedMembers()
    {
        var pending = await AddAsync("Asst Pending", "c1@example.com", "01700000005", "AS0005", 1964, null);
        pending.Status = Enums.MembershipStatus.Applied;
        var archived = await AddAsync("Asst Archived", "c2@example.com", "01700000006", "AS0006", 1964, null);
        archived.IsArchived = true;
        await _context.SaveChangesAsync();

        var result = await _service.AskAsync("find alumni 1964");

        result.FoundMembers.Should().BeNull();
        result.Answer.Should().Contain("couldn't find");
    }
}
