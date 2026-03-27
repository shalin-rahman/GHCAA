using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public class FamilyLinkServiceTests : TestBase
{
    private Mock<INotificationService> _mockNotifications = null!;
    private Mock<ICommunicationService> _mockCommunication = null!;
    private Mock<ILogger<FamilyLinkService>> _mockLogger = null!;
    private Mock<IConfiguration> _mockConfig = null!;
    private FamilyLinkService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockNotifications = new Mock<INotificationService>();
        _mockCommunication = new Mock<ICommunicationService>();
        _mockLogger = new Mock<ILogger<FamilyLinkService>>();
        _mockConfig = new Mock<IConfiguration>();

        _service = new FamilyLinkService(
            _context,
            _mockLogger.Object,
            _mockNotifications.Object,
            _mockCommunication.Object,
            _mockConfig.Object
        );
    }

    [Test]
    public async Task SendRequestAsync_ShouldCreateRequestAndNotify()
    {
        // Arrange
        var requester = await CreateTestMemberAsync("R001");
        var target = await CreateTestMemberAsync("T001");

        var dto = new SendFamilyLinkDto
        {
            TargetMembershipNumber = "T001",
            Relationship = Enums.RelationshipType.Spouse,
            Note = "Test Note"
        };

        // Act
        var result = await _service.SendRequestAsync(requester.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(Enums.FamilyLinkStatus.Requested);
        
        var request = await _context.FamilyLinkRequests.FirstOrDefaultAsync();
        request.Should().NotBeNull();
        request!.RequesterId.Should().Be(requester.Id);
        request.TargetMemberId.Should().Be(target.Id);

        _mockNotifications.Verify(x => x.CreateNotificationAsync(
            target.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
            
        _mockCommunication.Verify(x => x.SendIndividualEmailAsync(
            target.Id, "FAMILY_LINK_REQUEST", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RespondAsync_WhenApproved_ShouldAcceptAndLink()
    {
        // Arrange
        var requester = await CreateTestMemberAsync("R001");
        var target = await CreateTestMemberAsync("T001");
        
        var request = new FamilyLinkRequest
        {
            RequesterId = requester.Id,
            TargetMemberId = target.Id,
            Relationship = Enums.RelationshipType.Spouse,
            Status = Enums.FamilyLinkStatus.Requested
        };
        _context.FamilyLinkRequests.Add(request);
        await _context.SaveChangesAsync();

        var dto = new RespondFamilyLinkDto { RequestId = request.Id, Approve = true };

        // Act
        var success = await _service.RespondAsync(target.Id, dto);

        // Assert
        success.Should().BeTrue();
        var updatedRequest = await _context.FamilyLinkRequests.FindAsync(request.Id);
        updatedRequest!.Status.Should().Be(Enums.FamilyLinkStatus.Accepted);

        var updatedRequester = await _context.Members.FindAsync(requester.Id);
        // Relationship information is now in FamilyLinkRequests, not on Member directly

        _mockCommunication.Verify(x => x.SendIndividualEmailAsync(
            requester.Id, "FAMILY_LINK_ACCEPTED", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoveLinkAsync_ShouldDissolveConnection()
    {
        // Arrange
        var requester = await CreateTestMemberAsync("R001");
        var target = await CreateTestMemberAsync("T001");
        // Connection is handled by requests
        
        var request = new FamilyLinkRequest
        {
            RequesterId = requester.Id,
            TargetMemberId = target.Id,
            Relationship = Enums.RelationshipType.Spouse,
            Status = Enums.FamilyLinkStatus.Accepted
        };
        _context.FamilyLinkRequests.Add(request);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveLinkAsync(requester.Id, request.Id);

        // Assert
        result.Should().BeTrue();
        var updatedRequest = await _context.FamilyLinkRequests.FindAsync(request.Id);
        updatedRequest!.Status.Should().Be(Enums.FamilyLinkStatus.Cancelled);

        var updatedRequester = await _context.Members.FindAsync(requester.Id);
        // Connection removed status is in FamilyLinkRequests
    }

    private async Task<Member> CreateTestMemberAsync(string membershipNo)
    {
        var member = await CreateAndSaveTestMemberAsync($"Test {membershipNo}", $"test{membershipNo}@example.com", $"017{Guid.NewGuid().ToString("N").Substring(0, 8)}", $"NID{membershipNo}");
        member.MembershipNumber = membershipNo;
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }
}
