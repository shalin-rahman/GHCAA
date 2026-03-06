using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class GovernanceServiceTests : TestBase
    {
        private GovernanceService _service = null!;

        [SetUp]
        public void Setup()
        {
            _context.ECMembers.RemoveRange(_context.ECMembers);
            _context.ECPeriods.RemoveRange(_context.ECPeriods);
            _context.SaveChanges();
            _service = new GovernanceService(_context);
        }

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
            var updatedMember = await _context.Members.FindAsync(member.Id);
            updatedMember!.ECPosition.Should().Be(Enums.ECPosition.President);

            var ecMember = await _context.ECMembers.FirstOrDefaultAsync(em => em.MemberId == member.Id && em.ECPeriodId == period.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(Enums.ECPosition.President);
            ecMember.ChangeReason.Should().Be("Voted");
        }

        [Test]
        public async Task RemoveMemberFromCommitteeAsync_ShouldEndRecordAndResetProfile()
        {
            // Arrange
            var member = CreateMinimalMember("Test User 2");
            member.ECPosition = Enums.ECPosition.President;
            _context.Members.Add(member);
            
            var period = await _service.CreatePeriodAsync("Active Period", DateTime.UtcNow.AddDays(-1), null);
            await _service.ActivatePeriodAsync(period.Id);
            
            var ecMember = new ECMember { MemberId = member.Id, ECPeriodId = period.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow };
            _context.ECMembers.Add(ecMember);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoveMemberFromCommitteeAsync(ecMember.Id);

            // Assert
            var updatedMember = await _context.Members.FindAsync(member.Id);
            updatedMember!.ECPosition.Should().Be(Enums.ECPosition.None);

            var updatedECMember = await _context.ECMembers.FindAsync(ecMember.Id);
            updatedECMember!.EndDate.Should().NotBeNull();
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
                HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None",
                ProfessionalSector = "P",
                Designation = "D",
                GHCLastCertificatePassingYear = 2005,
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = Enums.Gender.Male,
                BloodGroup = Enums.BloodGroup.APositive,
                Status = Enums.MembershipStatus.Active
            };
        }
    }
}
