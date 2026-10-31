using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

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
            await _service.AssignMemberToRoleAsync(period.Id, member.Id, EcPositionIds.President, "Voted");

            // Assert
            var ecMember = await _context.ECMembers.FirstOrDefaultAsync(em => em.MemberId == member.Id && em.ECPeriodId == period.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(EcPositionIds.President);
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
            await _service.AssignMemberToRoleAsync(period.Id, member.Id, EcPositionIds.President, "Voted", notifyMember: true);

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

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveMemberFromCommitteeAsync(ecMember.Id, Enums.VacancyReason.Resigned, null);

            // Assert
            var updatedECMember = await _context.ECMembers.FindAsync(ecMember.Id);
            updatedECMember!.EndDate.Should().NotBeNull();
            updatedECMember.EndReason.Should().Be(Enums.VacancyReason.Resigned);

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
            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveMemberFromCommitteeAsync(ecMember.Id, Enums.VacancyReason.Resigned, null, notifyMember: true);

            // Assert
            _notificationService.Verify(n => n.CreateNotificationAsync(
                member.Id, It.IsAny<string>(), It.IsAny<string>(), Enums.NotificationType.CommitteeAssignment,
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private async Task<Member> AddSavedMemberAsync(string name, string nid)
        {
            var member = CreateMinimalMember(name);
            member.NID = nid;
            member.MobileNo = "017" + nid[^8..];
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        private async Task<(Member holder, GHCAA.Application.DTOs.ECPeriodDto period, ECMember seat)> SeedHeldPresidencyAsync()
        {
            var holder = await AddSavedMemberAsync("Current President", "9500000001");
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            var seat = new ECMember { MemberId = holder.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(seat);
            await _context.SaveChangesAsync();
            return (holder, period, seat);
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_RefusesSecondHolder_WithoutReplaceReason()
        {
            var (_, period, seat) = await SeedHeldPresidencyAsync();
            var other = await AddSavedMemberAsync("Would Be President", "9500000002");

            var act = () => _service.AssignMemberToRoleAsync(period.Id, other.Id, EcPositionIds.President, "Voted");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Current President*");
            (await _context.ECMembers.FindAsync(seat.Id))!.EndDate.Should().BeNull();
            _context.ECMembers.Count(em => em.MemberId == other.Id).Should().Be(0);
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_RefusesSameMemberTwice()
        {
            var (holder, period, _) = await SeedHeldPresidencyAsync();

            var act = () => _service.AssignMemberToRoleAsync(period.Id, holder.Id, EcPositionIds.President, "Again",
                endCurrentHolderReason: Enums.VacancyReason.Removed);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_WithReplaceReason_EndsOldHolderAndSeatsNewOne()
        {
            var (_, period, seat) = await SeedHeldPresidencyAsync();
            var other = await AddSavedMemberAsync("New President", "9500000003");

            await _service.AssignMemberToRoleAsync(period.Id, other.Id, EcPositionIds.President, "By-election",
                endCurrentHolderReason: Enums.VacancyReason.Died, endCurrentHolderNote: "Passed away in May");

            var old = await _context.ECMembers.FindAsync(seat.Id);
            old!.EndDate.Should().NotBeNull();
            old.EndReason.Should().Be(Enums.VacancyReason.Died);
            old.EndNote.Should().Be("Passed away in May");
            _context.ECMembers.Count(em => em.ECPeriodId == period.Id && em.Position == EcPositionIds.President && em.EndDate == null)
                .Should().Be(1);
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_OtherWithoutNote_IsRefused()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();

            var act = () => _service.RemoveMemberFromCommitteeAsync(seat.Id, Enums.VacancyReason.Other, "   ");

            await act.Should().ThrowAsync<InvalidOperationException>();
            (await _context.ECMembers.FindAsync(seat.Id))!.EndDate.Should().BeNull();
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_AlreadyEnded_IsRefused()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();
            await _service.RemoveMemberFromCommitteeAsync(seat.Id, Enums.VacancyReason.TermEnded, null);

            var act = () => _service.RemoveMemberFromCommitteeAsync(seat.Id, Enums.VacancyReason.Resigned, null);

            await act.Should().ThrowAsync<InvalidOperationException>();
            (await _context.ECMembers.FindAsync(seat.Id))!.EndReason.Should().Be(Enums.VacancyReason.TermEnded);
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_UnknownId_ReturnsFalse()
        {
            (await _service.RemoveMemberFromCommitteeAsync(999999, Enums.VacancyReason.Resigned, null)).Should().BeFalse();
        }

        [Category("FR-34")]
        [Test]
        public async Task GetCommitteeSeatsAsync_ShowsHolderAndVacancyReason()
        {
            var (_, period, seat) = await SeedHeldPresidencyAsync();
            var treasurer = await AddSavedMemberAsync("Former Treasurer", "9500000004");
            var treasurerSeat = new ECMember { MemberId = treasurer.Id, ECPeriodId = period.Id, Position = EcPositionIds.Treasurer, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(treasurerSeat);
            await _context.SaveChangesAsync();
            await _service.RemoveMemberFromCommitteeAsync(treasurerSeat.Id, Enums.VacancyReason.Resigned, "Moved abroad");

            var seats = (await _service.GetCommitteeSeatsAsync(period.Id)).ToList();

            seats.Should().HaveCount(_context.EcPositions.Count(p => p.IsActive));
            var president = seats.Single(s => s.PositionId == EcPositionIds.President);
            president.Holder!.Id.Should().Be(seat.Id);
            president.VacancyReason.Should().BeNull();
            var vacant = seats.Single(s => s.PositionId == EcPositionIds.Treasurer);
            vacant.Holder.Should().BeNull();
            vacant.VacancyReason.Should().Be(Enums.VacancyReason.Resigned);
            vacant.VacancyNote.Should().Be("Moved abroad");
            vacant.LastHolderName.Should().Be("Former Treasurer");
            vacant.VacatedOn.Should().NotBeNull();
            seats.Single(s => s.PositionId == EcPositionIds.GeneralSecretary).VacancyReason.Should().BeNull();
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_NoteOverMaxLength_IsRefused()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();

            var act = () => _service.RemoveMemberFromCommitteeAsync(seat.Id, Enums.VacancyReason.Resigned,
                new string('x', Constants.Governance.VacancyNoteMaxLength + 1));

            await act.Should().ThrowAsync<InvalidOperationException>();
            (await _context.ECMembers.FindAsync(seat.Id))!.EndDate.Should().BeNull();
        }

        [Category("FR-34")]
        [Test]
        public async Task RemoveMemberFromCommitteeAsync_UnknownReasonNumber_IsRefused()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();

            var act = () => _service.RemoveMemberFromCommitteeAsync(seat.Id, (Enums.VacancyReason)99, null);

            await act.Should().ThrowAsync<InvalidOperationException>();
            (await _context.ECMembers.FindAsync(seat.Id))!.EndDate.Should().BeNull();
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_UnknownPosition_IsRefused()
        {
            var (_, period, _) = await SeedHeldPresidencyAsync();
            var other = await AddSavedMemberAsync("No Seat", "9000000099");

            var act = () => _service.AssignMemberToRoleAsync(period.Id, other.Id, 999, null);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Category("FR-34")]
        [Test]
        public async Task GetCommitteeSeatsAsync_TwoOpenRows_ShowsTheNewestHolder()
        {
            var (_, period, _) = await SeedHeldPresidencyAsync();
            var later = await AddSavedMemberAsync("Later President", "9000000098");
            var laterSeat = new ECMember { MemberId = later.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow.AddDays(1) };
            _context.ECMembers.Add(laterSeat);
            await _context.SaveChangesAsync();

            var seats = await _service.GetCommitteeSeatsAsync(period.Id);

            seats.Single(s => s.PositionId == EcPositionIds.President).Holder!.Id.Should().Be(laterSeat.Id);
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

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow };
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
            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow };
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

            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow, IsArchived = true };
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

        [Category("FR-36")]
        [Test]
        public async Task VoteOnConstitutionAsync_ReturnsFalse_WhenConcurrentVoteWinsTheUniqueIndex()
        {
            var member = CreateMinimalMember("Racing Voter");
            member.MembershipType = Enums.MembershipType.General;
            _context.Members.Add(member);
            var constitution = new Constitution
            {
                Version = "v6.0",
                Content = "current",
                ChangeSummary = "test",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Constitutions.Add(constitution);
            await _context.SaveChangesAsync();

            // The competing vote lands after the alreadyVoted check but before this save,
            // which is the window two simultaneous requests share.
            var connection = _context.Database.GetDbConnection();
            var racer = new CompetingVoteInterceptor(connection, constitution.Id, member.Id);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(racer)
                .Options;
            await using var racingContext = new ApplicationDbContext(options);
            var service = new GovernanceService(racingContext, _notificationService.Object);

            var result = await service.VoteOnConstitutionAsync(constitution.Id, member.Id, isFor: true, comments: null);

            result.Should().BeFalse();
            racer.Fired.Should().BeTrue();
            (await _context.AmendmentVotes.CountAsync(v => v.MemberId == member.Id)).Should().Be(1);
        }

        private sealed class CompetingVoteInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
        {
            private readonly System.Data.Common.DbConnection _connection;
            private readonly int _constitutionId;
            private readonly int _memberId;
            public bool Fired { get; private set; }

            public CompetingVoteInterceptor(System.Data.Common.DbConnection connection, int constitutionId, int memberId)
            {
                _connection = connection;
                _constitutionId = constitutionId;
                _memberId = memberId;
            }

            public override async ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int>> SavingChangesAsync(
                Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData eventData,
                Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                if (!Fired)
                {
                    Fired = true;
                    var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
                    await using var other = new ApplicationDbContext(options);
                    other.AmendmentVotes.Add(new AmendmentVote { ConstitutionId = _constitutionId, MemberId = _memberId, IsFor = false, VotedAt = DateTime.UtcNow });
                    await other.SaveChangesAsync(cancellationToken);
                }
                return result;
            }
        }

        [Category("FR-34")]
        [Test]
        public async Task GetPositionsAsync_ReturnsTheSeededPositionsInDisplayOrder()
        {
            var positions = await _service.GetPositionsAsync(includeRetired: false);

            positions.Should().HaveCount(15);
            positions[0].Key.Should().Be("President");
            positions.Select(p => p.DisplayOrder).Should().BeInAscendingOrder();
            positions.Single(p => p.SignsAsHead).Id.Should().Be(EcPositionIds.President);
            positions.Single(p => p.SignsAsSecretary).Id.Should().Be(EcPositionIds.GeneralSecretary);
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePositionAsync_BuildsTheKeyFromTheName()
        {
            var created = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "  Welfare & Alumni-Relations Secretary 2 ", DisplayOrder = 20 });

            created.Id.Should().BeGreaterThan(EcPositionIds.InstitutionalRepresentative);
            created.Key.Should().Be("WelfareAlumniRelationsSecretary2");
            created.Name.Should().Be("Welfare & Alumni-Relations Secretary 2");
            created.IsActive.Should().BeTrue();
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePositionAsync_RefusesANameAlreadyInUse()
        {
            var act = () => _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Treasurer" });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePositionAsync_RefusesANameWithNoLettersOrDigits()
        {
            var act = () => _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "& - &" });

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Category("FR-34")]
        [Test]
        public async Task UpdatePositionAsync_MovingTheHeadFlagClearsItFromThePresident()
        {
            var updated = await _service.UpdatePositionAsync(EcPositionIds.VicePresident, new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Vice President", DisplayOrder = 2, SeatCount = 1, ShowOnBoard = true, SignsAsHead = true });

            updated!.SignsAsHead.Should().BeTrue();
            updated.Key.Should().Be("VicePresident");
            (await _context.EcPositions.SingleAsync(p => p.Id == EcPositionIds.President)).SignsAsHead.Should().BeFalse();
            (await _context.EcPositions.SingleAsync(p => p.Id == EcPositionIds.GeneralSecretary)).SignsAsSecretary.Should().BeTrue();
        }

        [Category("FR-34")]
        [Test]
        public async Task UpdatePositionAsync_ReturnsNullForAnUnknownId()
        {
            var updated = await _service.UpdatePositionAsync(999, new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Nobody" });

            updated.Should().BeNull();
        }

        [Category("FR-34")]
        [Test]
        public async Task SetPositionActiveAsync_RefusesToRetireAPositionWithACurrentHolder()
        {
            await SeedHeldPresidencyAsync();

            var act = () => _service.SetPositionActiveAsync(EcPositionIds.President, false);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*current holder*");
        }

        [Category("FR-34")]
        [Test]
        public async Task SetPositionActiveAsync_RetiredPositionLeavesTheList_AndCanBeRestored()
        {
            (await _service.SetPositionActiveAsync(EcPositionIds.LawSecretary, false)).Should().BeTrue();
            (await _service.GetPositionsAsync(includeRetired: false)).Should().NotContain(p => p.Id == EcPositionIds.LawSecretary);
            (await _service.GetPositionsAsync(includeRetired: true)).Should().Contain(p => p.Id == EcPositionIds.LawSecretary && !p.IsActive);

            (await _service.SetPositionActiveAsync(EcPositionIds.LawSecretary, true)).Should().BeTrue();
            (await _service.GetPositionsAsync(includeRetired: false)).Should().Contain(p => p.Id == EcPositionIds.LawSecretary);
        }

        [Category("FR-34")]
        [Test]
        public async Task DeletePositionAsync_RefusesAPositionWithPastTerms()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();
            seat.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var act = () => _service.DeletePositionAsync(EcPositionIds.President);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Retire it instead*");
        }

        [Category("FR-34")]
        [Test]
        public async Task DeletePositionAsync_RemovesAnUnusedPosition()
        {
            var created = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Advisor" });

            (await _service.DeletePositionAsync(created.Id)).Should().BeTrue();
            (await _context.EcPositions.AnyAsync(p => p.Id == created.Id)).Should().BeFalse();
            (await _service.DeletePositionAsync(created.Id)).Should().BeFalse();
        }

        [Category("FR-34")]
        [Test]
        public async Task AssignMemberToRoleAsync_TwoSeatPosition_TakesTwoHoldersThenRefusesAThird()
        {
            var position = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Executive Member", SeatCount = 2 });
            var period = await _service.CreatePeriodAsync("Two Seat Period", DateTime.UtcNow.AddDays(-1), null);
            var first = await AddSavedMemberAsync("First Holder", "9600000001");
            var second = await AddSavedMemberAsync("Second Holder", "9600000002");
            var third = await AddSavedMemberAsync("Third Holder", "9600000003");

            await _service.AssignMemberToRoleAsync(period.Id, first.Id, position.Id, "Voted");
            await _service.AssignMemberToRoleAsync(period.Id, second.Id, position.Id, "Voted");
            var act = () => _service.AssignMemberToRoleAsync(period.Id, third.Id, position.Id, "Voted",
                endCurrentHolderReason: Enums.VacancyReason.Resigned);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*All 2 Executive Member seats are held*");
            _context.ECMembers.Count(em => em.Position == position.Id && em.EndDate == null).Should().Be(2);
        }

        [Category("FR-34")]
        [Test]
        public async Task GetCommitteeSeatsAsync_TwoSeatPosition_ShowsOneHeldAndOneEmptySeat()
        {
            var position = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Executive Member", SeatCount = 2 });
            var period = await _service.CreatePeriodAsync("Two Seat Period", DateTime.UtcNow.AddDays(-1), null);
            var holder = await AddSavedMemberAsync("Only Holder", "9600000004");
            await _service.AssignMemberToRoleAsync(period.Id, holder.Id, position.Id, "Voted");

            var seats = (await _service.GetCommitteeSeatsAsync(period.Id)).Where(s => s.PositionId == position.Id).ToList();

            seats.Should().HaveCount(2);
            seats.Count(s => s.Holder != null).Should().Be(1);
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePositionAsync_RefusesANameThatDiffersOnlyInCase()
        {
            var act = () => _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "TREASURER" });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        }

        [Category("FR-34")]
        [TestCase(-1)]
        [TestCase(Constants.Governance.PositionMaxDisplayOrder + 1)]
        public async Task CreatePositionAsync_RefusesADisplayOrderOutOfRange(int order)
        {
            var act = () => _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Adviser", DisplayOrder = order });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Display order*");
        }

        [Category("FR-34")]
        [Test]
        public async Task UpdatePositionAsync_RefusesFewerSeatsThanCurrentHolders()
        {
            var position = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Executive Member", SeatCount = 2 });
            var period = await _service.CreatePeriodAsync("Two Seat Period", DateTime.UtcNow.AddDays(-1), null);
            await _service.AssignMemberToRoleAsync(period.Id, (await AddSavedMemberAsync("First Holder", "9600000011")).Id, position.Id, "Voted");
            await _service.AssignMemberToRoleAsync(period.Id, (await AddSavedMemberAsync("Second Holder", "9600000012")).Id, position.Id, "Voted");

            var act = () => _service.UpdatePositionAsync(position.Id, new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Executive Member", SeatCount = 1 });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("2 people hold*");
            (await _context.EcPositions.SingleAsync(p => p.Id == position.Id)).SeatCount.Should().Be(2);
        }

        [Category("FR-34")]
        [Test]
        public async Task UpdatePositionAsync_RefusesToClearTheOnlySignatoryFlag()
        {
            var act = () => _service.UpdatePositionAsync(EcPositionIds.President, new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "President", DisplayOrder = 1, SeatCount = 1, ShowOnBoard = true, SignsAsHead = false });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*need a signatory*");
        }

        [Category("FR-34")]
        [Test]
        public async Task SetPositionActiveAsync_RefusesToRetireASignatory()
        {
            var act = () => _service.SetPositionActiveAsync(EcPositionIds.GeneralSecretary, false);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*signs certificates*");
        }

        [Category("FR-34")]
        [Test]
        public async Task GetInvalidPositionRowsAsync_ListsATermWithNoPosition_AndRepairFixesIt()
        {
            var (_, _, seat) = await SeedHeldPresidencyAsync();
            // The FK would refuse a missing position, so switch it off to make the row an upgrade can leave.
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
            try
            {
                seat.Position = 0;
                await _context.SaveChangesAsync();
            }
            finally
            {
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");
            }

            var rows = await _service.GetInvalidPositionRowsAsync();

            rows.Should().ContainSingle(r => r.Kind == Constants.Governance.RepairKinds.CommitteeTerm && r.Id == seat.Id && r.Position == 0);

            var fixedRow = await _service.RepairPositionAsync(new GHCAA.Application.DTOs.RepairPositionRequest { Kind = Constants.Governance.RepairKinds.CommitteeTerm, Id = seat.Id, PositionId = EcPositionIds.President });

            fixedRow.Should().BeTrue();
            (await _service.GetInvalidPositionRowsAsync()).Should().BeEmpty();
        }

        [Category("FR-34")]
        [Test]
        public async Task RepairPositionAsync_RefusesAPositionWhoseSeatsAreAllHeld()
        {
            var (_, period, _) = await SeedHeldPresidencyAsync();
            var other = await AddSavedMemberAsync("Second Claimant", "9600000013");
            _context.ECMembers.Add(new ECMember { ECPeriodId = period.Id, MemberId = other.Id, Position = EcPositionIds.Treasurer, StartDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            var term = await _context.ECMembers.SingleAsync(em => em.MemberId == other.Id);

            var act = () => _service.RepairPositionAsync(new GHCAA.Application.DTOs.RepairPositionRequest { Kind = Constants.Governance.RepairKinds.CommitteeTerm, Id = term.Id, PositionId = EcPositionIds.President });

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Every President seat*");
        }

        [Category("FR-34")]
        [Test]
        public async Task CertificateCaptions_FollowARenamedSignatory()
        {
            await _service.UpdatePositionAsync(EcPositionIds.GeneralSecretary, new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Secretary General", DisplayOrder = 3, SeatCount = 1, ShowOnBoard = true, SignsAsSecretary = true });

            var (head, secretary) = await IDCardService.GetSignatureCaptionsAsync(_context);

            head.Should().Be("President");
            secretary.Should().Be("Secretary General");
        }

        [Category("FR-34")]
        [Test]
        public async Task CertificateCaptions_FallBackWhenNoPositionSigns()
        {
            // The service refuses to clear the last signatory, so clear the flags directly.
            await _context.EcPositions.ExecuteUpdateAsync(s => s.SetProperty(p => p.SignsAsHead, false).SetProperty(p => p.SignsAsSecretary, false));

            var (head, secretary) = await IDCardService.GetSignatureCaptionsAsync(_context);

            head.Should().Be(Constants.Governance.DefaultHeadCaption);
            secretary.Should().Be(Constants.Governance.DefaultSecretaryCaption);
        }

        [Category("FR-34")]
        [Test]
        public async Task RepairPositionAsync_UnknownKindOrRow_ReturnsFalse()
        {
            (await _service.RepairPositionAsync(new GHCAA.Application.DTOs.RepairPositionRequest { Kind = "other", Id = 1, PositionId = EcPositionIds.President })).Should().BeFalse();
            (await _service.RepairPositionAsync(new GHCAA.Application.DTOs.RepairPositionRequest { Kind = Constants.Governance.RepairKinds.ElectionSeat, Id = 999, PositionId = EcPositionIds.President })).Should().BeFalse();
        }

        [Category("FR-34")]
        [Test]
        public async Task GetCommitteeSeatsAsync_LeavesOutARetiredPosition_UnlessSomeoneHeldItInThatPeriod()
        {
            var period = await _service.CreatePeriodAsync("Retired Seats Period", DateTime.UtcNow.AddDays(-1), null);
            var adviser = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Adviser" });
            var holder = await AddSavedMemberAsync("Past Adviser", "9600000021");
            await _service.AssignMemberToRoleAsync(period.Id, holder.Id, adviser.Id, "Voted");
            var term = await _context.ECMembers.SingleAsync(em => em.MemberId == holder.Id);
            term.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _service.SetPositionActiveAsync(adviser.Id, false);
            await _service.SetPositionActiveAsync(EcPositionIds.LawSecretary, false);

            var seats = (await _service.GetCommitteeSeatsAsync(period.Id)).ToList();

            seats.Should().NotContain(s => s.PositionId == EcPositionIds.LawSecretary);
            seats.Should().ContainSingle(s => s.PositionId == adviser.Id).Which.LastHolderName.Should().Be("Past Adviser");
        }

        [Category("FR-34")]
        [Test]
        public async Task CreatePositionAsync_TakingBothSignatoryFlags_ClearsThemFromPresidentAndSecretary()
        {
            var created = await _service.CreatePositionAsync(new GHCAA.Application.DTOs.SaveEcPositionRequest { Name = "Convenor", SignsAsHead = true, SignsAsSecretary = true });

            var flags = await _context.EcPositions.Where(p => p.SignsAsHead || p.SignsAsSecretary).ToListAsync();
            flags.Should().ContainSingle().Which.Id.Should().Be(created.Id);
            created.SignsAsHead.Should().BeTrue();
            created.SignsAsSecretary.Should().BeTrue();
        }

        // 37.14m. ER-34 handover ticks belong to the committee term.
        [Category("FR-34")]
        [Test]
        public async Task SetHandoverCheckAsync_TickThenUntick_KeepsOneRowPerItem()
        {
            var period = await _service.CreatePeriodAsync("Handover", DateTime.UtcNow, null);
            var user = new User { Username = "secretary", PasswordHash = "x" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var tick = new GHCAA.Application.DTOs.SetHandoverCheckRequest { Item = Enums.HandoverCheckItem.MinuteBooks, IsChecked = true };
            (await _service.SetHandoverCheckAsync(period.Id, tick, user.Id)).Should().BeTrue();
            tick.IsChecked = false;
            (await _service.SetHandoverCheckAsync(period.Id, tick, user.Id)).Should().BeTrue();

            var checks = await _service.GetHandoverChecksAsync(period.Id);
            checks.Should().ContainSingle().Which.Should().Match<GHCAA.Application.DTOs.HandoverCheckDto>(c =>
                c.Item == Enums.HandoverCheckItem.MinuteBooks && !c.IsChecked && c.SetByUserId == user.Id);
        }

        [Category("FR-34")]
        [Test]
        public async Task HandoverChecks_MissingTermOrUnknownItem_AreRefused()
        {
            (await _service.GetHandoverChecksAsync(9999)).Should().BeNull();
            (await _service.SetHandoverCheckAsync(9999, new GHCAA.Application.DTOs.SetHandoverCheckRequest { Item = Enums.HandoverCheckItem.Constitution }, 1)).Should().BeFalse();

            var period = await _service.CreatePeriodAsync("Handover", DateTime.UtcNow, null);
            var unknown = new GHCAA.Application.DTOs.SetHandoverCheckRequest { Item = (Enums.HandoverCheckItem)99 };
            await FluentActions.Awaiting(() => _service.SetHandoverCheckAsync(period.Id, unknown, 1)).Should().ThrowAsync<InvalidRequestException>();
            (await _service.GetHandoverChecksAsync(period.Id)).Should().BeEmpty();
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

