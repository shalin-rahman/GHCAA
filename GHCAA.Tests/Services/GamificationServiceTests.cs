using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services;

[TestFixture]
public class GamificationServiceTests : TestBase
{
    private GamificationService _service = null!;
    private Mock<IActivityService> _activityMock = null!;
    private Mock<ILogger<GamificationService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _activityMock = new Mock<IActivityService>();
        _loggerMock = new Mock<ILogger<GamificationService>>();
        _service = new GamificationService(_context, _activityMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AwardPointsAsync_WithValidConfig_ShouldIncreaseMemberPoints()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync();
        var config = new GamificationConfig { ActivityCode = "TEST_ACT", Name = "Test Activity", Points = 50, IsActive = true };
        _context.GamificationConfigs.Add(config);
        await _context.SaveChangesAsync();

        // Act
        await _service.AwardPointsAsync(member.Id, "TEST_ACT");

        // Assert
        var updatedMember = await _context.Members.FindAsync(member.Id);
        updatedMember!.ContributionPoints.Should().Be(50);
        _activityMock.Verify(x => x.LogActivityAsync(member.Id, "PointsAwarded", It.Is<string>(s => s.Contains("50 pts")), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AwardPointsAsync_WhenInactive_ShouldNotAwardPoints()
    {
        // Arrange
        var member = await CreateAndSaveTestMemberAsync();
        var config = new GamificationConfig { ActivityCode = "INACTIVE_ACT", Name = "Inactive", Points = 100, IsActive = false };
        _context.GamificationConfigs.Add(config);
        await _context.SaveChangesAsync();

        // Act
        await _service.AwardPointsAsync(member.Id, "INACTIVE_ACT");

        // Assert
        var updatedMember = await _context.Members.FindAsync(member.Id);
        updatedMember!.ContributionPoints.Should().Be(0);
    }

    [Test]
    public async Task GetLeaderboardAsync_ShouldReturnTopMembersOrderedByPoints()
    {
        // Arrange
        var m1 = await CreateAndSaveTestMemberAsync("User 1", "u1@e.com", "011", "1");
        var m2 = await CreateAndSaveTestMemberAsync("User 2", "u2@e.com", "012", "2");
        var m3 = await CreateAndSaveTestMemberAsync("User 3", "u3@e.com", "013", "3");

        m1.ContributionPoints = 100;
        m2.ContributionPoints = 300;
        m3.ContributionPoints = 200;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLeaderboardAsync(10);

        // Assert
        result.Should().HaveCount(3);
        result.ElementAt(0).MemberId.Should().Be(m2.Id);
        result.ElementAt(0).Rank.Should().Be(1);
        result.ElementAt(1).MemberId.Should().Be(m3.Id);
        result.ElementAt(1).Rank.Should().Be(2);
        result.ElementAt(2).MemberId.Should().Be(m1.Id);
        result.ElementAt(2).Rank.Should().Be(3);
    }

    [Test]
    public async Task UpdateConfigAsync_ShouldModifyExistingConfig()
    {
        // Arrange
        var config = new GamificationConfig { ActivityCode = "UP_ACT", Name = "Up", Points = 10, IsActive = true };
        _context.GamificationConfigs.Add(config);
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateConfigAsync(config.Id, 25, false);

        // Assert
        var updated = await _context.GamificationConfigs.FindAsync(config.Id);
        updated!.Points.Should().Be(25);
        updated.IsActive.Should().BeFalse();
    }
}
