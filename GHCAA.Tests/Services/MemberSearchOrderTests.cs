using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        var service = new FamilyLinkService(_context, Mock.Of<ILogger<FamilyLinkService>>(), Mock.Of<INotificationService>(),
            Mock.Of<ICommunicationService>(), Mock.Of<IConfiguration>(), Mock.Of<IOrgConfigService>());

        var result = await service.SearchByNameAsync("Karim", excludeMemberId: 0);

        result.Select(m => m.FullName).Should().Equal("Karim Ahmed", "Karim Malik", "Karim Zaman");
    }

    // Since 84.21 the family search ignores case and spaces around the term.
    // The old FamilyService matched the raw term, so "karim " found nobody.
    [Test]
    [Category("FR-019")]
    public async Task FamilySearchByName_IgnoresCaseAndSurroundingSpaces()
    {
        await SeedMembersAsync("Karim Zaman", "Abid Hasan");

        var result = await NewFamilyLinkService().SearchByNameAsync("  karim ", excludeMemberId: 0);

        result.Select(m => m.FullName).Should().Equal("Karim Zaman");
    }

    [Test]
    [Category("FR-019")]
    public async Task FamilySearchByName_SkipsPrivateInactiveAndCaller()
    {
        await SeedMembersAsync("Karim Public", "Karim Private", "Karim Inactive", "Karim Caller");
        var members = _context.Members.ToList();
        members.Single(m => m.FullName == "Karim Private").IsFamilyPublic = false;
        members.Single(m => m.FullName == "Karim Inactive").Status = Enums.MembershipStatus.InactivePayment;
        await _context.SaveChangesAsync();
        var caller = members.Single(m => m.FullName == "Karim Caller");

        var result = await NewFamilyLinkService().SearchByNameAsync("Karim", caller.Id);

        result.Select(m => m.FullName).Should().Equal("Karim Public");
    }

    [Test]
    [Category("FR-019")]
    public async Task FamilySearchByName_ReturnsAtMostTwenty()
    {
        await SeedMembersAsync(Enumerable.Range(0, 22).Select(i => $"Karim {i:D2}").ToArray());

        var result = await NewFamilyLinkService().SearchByNameAsync("Karim", excludeMemberId: 0);

        result.Should().HaveCount(20);
    }

    private FamilyLinkService NewFamilyLinkService() =>
        new(_context, Mock.Of<ILogger<FamilyLinkService>>(), Mock.Of<INotificationService>(),
            Mock.Of<ICommunicationService>(), Mock.Of<IConfiguration>(), Mock.Of<IOrgConfigService>());

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
