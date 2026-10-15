using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using Moq;

namespace GHCAA.Tests.Services;

// Both searches cap their results with Take. Without an OrderBy, EF warns (10102) and which
// members come back is up to the database.
[TestFixture]
public class MemberSearchOrderTests : TestBase
{
    private async Task SeedMembersAsync(params string[] names)
    {
        for (var i = 0; i < names.Length; i++)
        {
            var member = await CreateAndSaveTestMemberAsync(names[i], $"order{i}@example.com", $"0170000{i:D4}", $"NIDORDER{i}");
            member.IsFamilyPublic = true;
        }
        await _context.SaveChangesAsync();
    }

    [Test]
    [Category("FR-019")]
    public async Task FamilySearchByName_ReturnsMatchesSortedByName()
    {
        await SeedMembersAsync("Karim Zaman", "Karim Ahmed", "Karim Malik");
        var service = new FamilyService(_context, Mock.Of<INotificationService>(), Mock.Of<IRealTimeService>());

        var result = await service.SearchByNameAsync("Karim", excludeMemberId: 0);

        result.Select(m => m.FullName).Should().Equal("Karim Ahmed", "Karim Malik", "Karim Zaman");
    }

    [Test]
    public async Task AssistantMemberSearch_ReturnsMembersSortedByName()
    {
        await SeedMembersAsync("Zara Khan", "Abid Hasan", "Mina Roy");
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto());
        var service = new AssistantService(_context, orgConfig.Object);

        var result = await service.AskAsync("find members");

        result.FoundMembers.Select(m => m.FullName).Should().Equal("Abid Hasan", "Mina Roy", "Zara Khan");
    }
}
