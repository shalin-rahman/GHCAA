using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class MemberService_EC_Tests : TestBase
    {
        private MemberService _service = null!;

        [SetUp]
        public void Setup()
        {
            // Minimal mocks
            var storage = new Mock<IFileStorageService>();

            var otp = new Mock<IOtpService>();
            var email = new Mock<IEmailService>();
            var user = new Mock<IUserService>();
            var comm = new Mock<ICommunicationService>();
            var logger = new Mock<ILogger<MemberService>>();
            var activity = new Mock<IActivityService>();
            var notify = new Mock<INotificationService>();
            var config = new Mock<IConfiguration>();
            var gamification = new Mock<IGamificationService>();
            var financials = new Mock<IFinancialService>();
            var orgConfig = new Mock<IOrgConfigService>();

            _service = new MemberService(_context, storage.Object, otp.Object, email.Object, user.Object, comm.Object, logger.Object, activity.Object, notify.Object, config.Object, gamification.Object, financials.Object, new Mock<IRealTimeService>().Object, orgConfig.Object, new Mock<ITokenService>().Object);

            if (!_context.ECPeriods.Any(p => p.Title == "Interim Executive Committee"))
            {
                _context.ECPeriods.Add(new ECPeriod { Title = "Interim Executive Committee", StartDate = DateTime.UtcNow.AddYears(-1), IsActive = true });
                _context.SaveChanges();
            }
        }

        [Test]
        public async Task AdminUpdateMemberAsync_ShouldCreateHistoricalRecord_WhenPositionChanges()
        {
            // Arrange
            var period = await _context.ECPeriods.FirstOrDefaultAsync(p => p.IsActive);
            period.Should().NotBeNull();

            var member = await CreateAndSaveTestMemberAsync("Tester");

            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto
                {
                    PeriodTitle = "Interim Executive Committee",
                    Position = Enums.ECPosition.President,
                    StartDate = DateTime.UtcNow,
                    IsCurrent = true
                }
            };

            // Act
            await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            // Assert
            var ecMember = await _context.ECMembers.Include(em => em.ECPeriod).FirstOrDefaultAsync(em => em.MemberId == member.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(Enums.ECPosition.President);
            ecMember.ECPeriod!.Title.Should().Be("Interim Executive Committee");
        }

        [Test]
        public async Task AdminUpdateMemberAsync_ShouldEndPreviousRecord_WhenPositionChangesMidTerm()
        {
            // Arrange
            var period = await _context.ECPeriods.FirstOrDefaultAsync(p => p.IsActive);
            period.Should().NotBeNull();

            var member = await CreateAndSaveTestMemberAsync("Tester");

            var oldRecord = new ECMember { MemberId = member.Id, ECPeriodId = period!.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow.AddMonths(-2) };
            _context.ECMembers.Add(oldRecord);
            await _context.SaveChangesAsync();

            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>(); // EMPTY history essentially removes current role if the logic wipes and replaces.

            updateDto.ECHistory.Add(new ECHistoryDto
            {
                PeriodTitle = "Interim Executive Committee",
                Position = Enums.ECPosition.President,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow,
                ChangeReason = "Resigned"
            });

            // Act
            await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            // Assert
            var records = await _context.ECMembers.Where(em => em.MemberId == member.Id).ToListAsync();
            records.Should().HaveCount(1);
            records[0].EndDate.Should().NotBeNull();
            records[0].ChangeReason.Should().Be("Resigned");
        }

        private AdminMemberUpdateDto CreateUpdateDto(Member m)
        {
            return new AdminMemberUpdateDto
            {
                FullName = m.FullName,
                Email = m.Email,
                MobileNo = m.MobileNo,
                NID = m.NID,
                FatherName = m.FatherName,
                MotherName = m.MotherName,
                PresentAddress = m.PresentAddress,
                PermanentAddress = m.PermanentAddress,
                DateOfBirth = m.DateOfBirth,
                Gender = m.Gender,
                BloodGroup = m.BloodGroup,
                EmergencyContactName = m.EmergencyContactName,
                EmergencyContactRelation = m.EmergencyContactRelation,
                EmergencyContactPhone = m.EmergencyContactPhone,
                MembershipType = m.MembershipType,
                Category = m.Category,
                Status = m.Status,
                IsVerified = m.IsVerified
            };
        }
    }
}
