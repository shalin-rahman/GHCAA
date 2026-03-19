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
            var fileRepo = new Mock<IFileUploadRepository>();
            var otp = new Mock<IOtpService>();
            var email = new Mock<IEmailService>();
            var user = new Mock<IUserService>();
            var comm = new Mock<ICommunicationService>();
            var logger = new Mock<ILogger<MemberService>>();
            var activity = new Mock<IActivityService>();
            var notify = new Mock<INotificationService>();
            var config = new Mock<IConfiguration>();

            _service = new MemberService(_context, storage.Object, fileRepo.Object, otp.Object, email.Object, user.Object, comm.Object, logger.Object, activity.Object, notify.Object, config.Object);
        
            if (!_context.ECPeriods.Any())
            {
                _context.ECPeriods.Add(new ECPeriod { Title = "Test Period", StartDate = DateTime.UtcNow, IsActive = true });
                _context.SaveChanges();
            }
        }

        [Test]
        public async Task AdminUpdateMemberAsync_ShouldCreateHistoricalRecord_WhenPositionChanges()
        {
            // Arrange
            var period = await _context.ECPeriods.FirstOrDefaultAsync(p => p.IsActive);
            period.Should().NotBeNull();

            var member = CreateMinimalMember("Tester");
            
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto
                {
                    PeriodTitle = "Test Period",
                    Position = Enums.ECPosition.President,
                    StartDate = DateTime.UtcNow,
                    IsCurrent = true
                }
            };

            // Act
            await _service.AdminUpdateMemberAsync(member.Id, updateDto);

            // Assert
            var ecMember = await _context.ECMembers.Include(em => em.ECPeriod).FirstOrDefaultAsync(em => em.MemberId == member.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(Enums.ECPosition.President);
            ecMember.ECPeriod!.Title.Should().Be("Test Period");
        }

        [Test]
        public async Task AdminUpdateMemberAsync_ShouldEndPreviousRecord_WhenPositionChangesMidTerm()
        {
            // Arrange
            var period = await _context.ECPeriods.FirstOrDefaultAsync(p => p.IsActive);
            period.Should().NotBeNull();

            var member = CreateMinimalMember("Tester");
            
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var oldRecord = new ECMember { MemberId = member.Id, ECPeriodId = period!.Id, Position = Enums.ECPosition.President, StartDate = DateTime.UtcNow.AddMonths(-2) };
            _context.ECMembers.Add(oldRecord);
            await _context.SaveChangesAsync();

            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>(); // EMPTY history essentially removes current role if the logic wipes and replaces.
            // Wait, MemberService.cs:807-808 removes ALL existing.
            // So if I send an empty list, it wipes history. 
            // If I want to "End" it but keep it in history, I should send it with an EndDate.

            updateDto.ECHistory.Add(new ECHistoryDto 
            {
                PeriodTitle = "Test Period",
                Position = Enums.ECPosition.President,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow,
                ChangeReason = "Resigned"
            });

            // Act
            await _service.AdminUpdateMemberAsync(member.Id, updateDto);

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
                FullName = m.FullName, Email = m.Email, MobileNo = m.MobileNo, NID = m.NID,
                FatherName = m.FatherName, MotherName = m.MotherName, PresentAddress = m.PresentAddress,
                PermanentAddress = m.PermanentAddress, MembershipType = "General", Category = "None",
//                 GHCLastCertificatePassingYear = m.GHCLastCertificatePassingYear, GHCLastCertificate = m.GHCLastCertificate,
//                 GHCLastCertificateGroup = m.GHCLastCertificateGroup, GHCLastCertificateSubject = m.GHCLastCertificateSubject,
//                 HighestCertificate = m.HighestCertificate, HighestCertificateGroup = m.HighestCertificateGroup, HighestCertificateSubject = m.HighestCertificateSubject,
//                 ProfessionalSector = m.ProfessionalSector,
//                 Designation = m.Designation
            };
        }

        private Member CreateMinimalMember(string name)
        {
            return new Member
            {
                FullName = name,
                Email = $"{name}@test.com",
                MobileNo = "01700000000",
                NID = "1234567890",
                FatherName = "F", MotherName = "M",
                PresentAddress = "A", PermanentAddress = "A",
                EmergencyContactName = "EC", EmergencyContactRelation = "Brother", EmergencyContactPhone = "01800000000",
//                 HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "P", Designation = "D",
//                 GHCLastCertificatePassingYear = 2005,
                DateOfBirth = new DateTime(1990,1,1),
                Gender = Enums.Gender.Male,
                BloodGroup = Enums.BloodGroup.APositive
            };
        }
    }
}
