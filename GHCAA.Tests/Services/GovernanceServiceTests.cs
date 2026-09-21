using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class GovernanceServiceTests : TestBase
    {
        private GovernanceService _service = null!;
        private Mock<INotificationService> _notificationService = null!;

        [SetUp]
        public void Setup()
        {
            _context.ECMembers.RemoveRange(_context.ECMembers);
            _context.ECPeriods.RemoveRange(_context.ECPeriods);
            _context.SaveChanges();
            _notificationService = new Mock<INotificationService>();
            _service = new GovernanceService(_context, _notificationService.Object);
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePeriodAsync_ShouldAddPeriod()
        {
            // Act
            var period = await _service.CreatePeriodAsync("Test Period", DateTime.UtcNow, null);

            // Assert
            period.Id.Should().BeGreaterThan(0);
            period.Title.Should().Be("Test Period");
            _context.ECPeriods.Count().Should().Be(1); // Cleared in setup, so 0+1=1
        }

        [Category("FR-34")]
        [Test]
        public async Task ActivatePeriodAsync_ShouldDeactivateOthers()
        {
            // Arrange
            // Past period
            var p1 = await _service.CreatePeriodAsync("P1", DateTime.UtcNow.AddYears(-3), DateTime.UtcNow.AddYears(-1));
            // Current period
            var p2 = await _service.CreatePeriodAsync("P2", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(2));

            p1.IsActive = true;
            await _context.SaveChangesAsync();

            // Act
            await _service.ActivatePeriodAsync(p2.Id);

            // Assert
            var updatedP1 = await _context.ECPeriods.FindAsync(p1.Id);
            var updatedP2 = await _context.ECPeriods.FindAsync(p2.Id);

            updatedP1!.IsActive.Should().BeFalse();
            updatedP2!.IsActive.Should().BeTrue();
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_ShouldCreateRecordAndSyncProfile()
        {
            // Arrange
            var member = CreateMinimalMember("Test User");
            _context.Members.Add(member);

            // Period must cover current date to be activated
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            await _service.ActivatePeriodAsync(period.Id);
            await _context.SaveChangesAsync();

            // Act
            await _service.AssignMemberToRoleAsync(period.Id, member.Id, (int)Enums.ECPosition.President, "Voted");

            // Assert
            var ecMember = await _context.ECMembers.FirstOrDefaultAsync(em => em.MemberId == member.Id && em.ECPeriodId == period.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(Enums.ECPosition.President);
            ecMember.ChangeReason.Should().Be("Voted");

            // 82.52: default is off — nothing notified this session before this item existed.
            _notificationService.Verify(n => n.CreateNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_NotifiesMember_WhenOptedIn()
        {
            // Arrange
            var member = CreateMinimalMember("Notified User");
            _context.Members.Add(member);
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            await _context.SaveChangesAsync();

            // Act
            await _service.AssignMemberToRoleAsync(period.Id, member.Id, (int)Enums.ECPosition.President, "Voted", notifyMember: true);

            // Assert
            _notificationService.Verify(n => n.CreateNotificationAsync(
                member.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.CommitteeAssignment,
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_ShouldEndRecordAndResetProfile()
        {
            // Arrange
            var member = CreateMinimalMember("Test User 2");
            _context.Members.Add(member);

            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            await _service.ActivatePeriodAsync(period.Id);

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveMemberFromCommitteeAsync(ecMember.Id);

            // Assert
            var updatedECMember = await _context.ECMembers.FindAsync(ecMember.Id);
            updatedECMember!.EndDate.Should().NotBeNull();

            // 82.52: default is off.
            _notificationService.Verify(n => n.CreateNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_NotifiesMember_WhenOptedIn()
        {
            // Arrange
            var member = CreateMinimalMember("Notified User 2");
            _context.Members.Add(member);
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveMemberFromCommitteeAsync(ecMember.Id, notifyMember: true);

            // Assert
            _notificationService.Verify(n => n.CreateNotificationAsync(
                member.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.CommitteeAssignment,
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // 82.29: DeleteECMemberAsync is the separate "this row should never have existed"
        // path (wrong member added), distinct from RemoveMemberFromCommitteeAsync above (a term
        // ending). ECMember is Class A, so this must soft-delete with an actor, not Remove().
        [Category("FR-34")]
        [Test]
        public async Task DeleteECMemberAsync_SoftDeletesWithActorAndTimestamp()
        {
            // Arrange
            var member = CreateMinimalMember("Wrongly Added");
            _context.Members.Add(member);
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteECMemberAsync(ecMember.Id, adminId: 9);

            // Assert
            result.Should().BeTrue();
            var stored = await _context.ECMembers.IgnoreQueryFilters().FirstAsync(em => em.Id == ecMember.Id);
            stored.IsArchived.Should().BeTrue();
            stored.DeletedByAdminId.Should().Be(9);
            stored.DeletedAt.Should().NotBeNull();

            // The row must disappear from ordinary reads once deleted (the query filter), the same
            // way a soft-deleted PaymentHistory row disappears from ordinary financial queries.
            (await _context.ECMembers.FirstOrDefaultAsync(em => em.Id == ecMember.Id)).Should().BeNull();

            // 82.52: default is off — this is the "wrong entry" correction path, rarely wanted.
            _notificationService.Verify(n => n.CreateNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-34")]
        [Test]
        public async Task DeleteECMemberAsync_NotifiesMember_WhenOptedIn()
        {
            // Arrange
            var member = CreateMinimalMember("Notified User 3");
            _context.Members.Add(member);
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteECMemberAsync(ecMember.Id, adminId: 9, notifyMember: true);

            // Assert
            _notificationService.Verify(n => n.CreateNotificationAsync(
                member.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.CommitteeAssignment,
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-34")]
        [Test]
        public async Task DeleteECMemberAsync_ReturnsFalse_WhenAlreadyDeleted()
        {
            // Arrange
            var member = CreateMinimalMember("Already Deleted");
            _context.Members.Add(member);
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow, IsArchived = true };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteECMemberAsync(ecMember.Id, adminId: 9);

            // Assert
            result.Should().BeFalse();
        }

        [Category("FR-34")]
        [Test]
        public async Task UpdatePeriodAsync_ShouldModifyFieldsAndForceUtc()
        {
            // Arrange
            var period = await _service.CreatePeriodAsync("Old", DateTime.UtcNow.AddDays(-1), null);
            var newStart = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
            var newEnd = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Local);

            // Act
            await _service.UpdatePeriodAsync(period.Id, "Updated", newStart, newEnd, false);

            // Assert
            var updated = await _context.ECPeriods.FindAsync(period.Id);
            updated!.Title.Should().Be("Updated");
            updated.StartDate.Kind.Should().Be(DateTimeKind.Utc);
            updated.EndDate.Should().NotBeNull();
            updated.EndDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Category("FR-32")]
        [Category("DC-16")]
        [Test]
        public async Task GetActiveConstitutionAsync_ReturnsLatestByEffectiveDate_NotInsertionOrder()
        {
            // Arrange: insert the newer row first, so a naive "first active row found" read
            // would return the wrong one. DC-16 requires the latest ratified version by
            // effective date, not by row order.
            var newer = new Constitution
            {
                Version = "v5.0",
                Content = "newer",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            var older = new Constitution
            {
                Version = "v4.2",
                Content = "older",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow.AddYears(-1),
                IsActive = true
            };
            _context.Constitutions.Add(newer);
            _context.Constitutions.Add(older);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveConstitutionAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Version.Should().Be("v5.0");
        }

        [Category("FR-33")]
        [Category("DC-16")]
        [Test]
        public async Task ActivateConstitutionAsync_SupersedesPreviousVersion_WithoutDeletingIt()
        {
            // Arrange
            var previous = new Constitution
            {
                Version = "v4.2",
                Content = "old",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow.AddYears(-1),
                IsActive = true
            };
            var next = new Constitution
            {
                Version = "v5.0",
                Content = "new",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow,
                IsActive = false
            };
            _context.Constitutions.Add(previous);
            _context.Constitutions.Add(next);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ActivateConstitutionAsync(next.Id);

            // Assert: DC-16 is supersede-not-delete, so the previous version must still be
            // in the table, just no longer active.
            result.Should().BeTrue();
            var storedPrevious = await _context.Constitutions.FindAsync(previous.Id);
            storedPrevious.Should().NotBeNull();
            storedPrevious!.IsActive.Should().BeFalse();
            storedPrevious.SupersededDate.Should().NotBeNull();

            var storedNext = await _context.Constitutions.FindAsync(next.Id);
            storedNext!.IsActive.Should().BeTrue();
            storedNext.SupersededDate.Should().BeNull();
        }

        [Category("FR-36")]
        [Category("DC-03")]
        [Test]
        public async Task VoteOnConstitutionAsync_RefusesNonVotingTierMember()
        {
            // Arrange: DC-03 restricts amendment voting to Founding, Executive and General
            // members. Associate is not one of those tiers, so the vote must be refused and
            // nothing persisted.
            var member = CreateMinimalMember("Associate Voter");
            member.MembershipType = Enums.MembershipType.Associate;
            _context.Members.Add(member);
            var constitution = new Constitution
            {
                Version = "v5.0",
                Content = "current",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Constitutions.Add(constitution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.VoteOnConstitutionAsync(constitution.Id, member.Id, isFor: true, comments: null);

            // Assert
            result.Should().BeFalse();
            (await _context.AmendmentVotes.AnyAsync(v => v.MemberId == member.Id)).Should().BeFalse();
        }

        [Category("FR-36")]
        [Category("DC-03")]
        [Test]
        public async Task VoteOnConstitutionAsync_AcceptsVotingTierMember()
        {
            // Arrange: General is one of the three voting tiers DC-03 names.
            var member = CreateMinimalMember("General Voter");
            member.MembershipType = Enums.MembershipType.General;
            _context.Members.Add(member);
            var constitution = new Constitution
            {
                Version = "v5.0",
                Content = "current",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Constitutions.Add(constitution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.VoteOnConstitutionAsync(constitution.Id, member.Id, isFor: true, comments: "Agreed");

            // Assert
            result.Should().BeTrue();
            var vote = await _context.AmendmentVotes.FirstOrDefaultAsync(v => v.MemberId == member.Id);
            vote.Should().NotBeNull();
            vote!.ConstitutionId.Should().Be(constitution.Id);
            vote.IsFor.Should().BeTrue();
        }

        private Member CreateMinimalMember(string name)
        {
            return new Member
            {
                FullName = name,
                Email = $"{name.Replace(" ", "").ToLower()}@test.com",
                MobileNo = "01700000000",
                NID = "1234567890",
                FatherName = "F",
                MotherName = "M",
                PresentAddress = "A",
                PermanentAddress = "A",
                EmergencyContactName = "EC",
                EmergencyContactRelation = "Brother",
                EmergencyContactPhone = "01800000000",
                AcademicHistory = new List<AcademicRecord> { new AcademicRecord { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsOrgProfile = true } },
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = Enums.Gender.Male,
                BloodGroup = Enums.BloodGroup.APositive,
                Status = Enums.MembershipStatus.Active
            };
        }
    }
}

