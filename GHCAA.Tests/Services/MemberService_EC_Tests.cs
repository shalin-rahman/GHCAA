using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

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
            var appSettings = Options.Create(new AppSettingsOptions());
            var gamification = new Mock<IGamificationService>();
            var financials = new Mock<IFinancialService>();
            var orgConfig = new Mock<IOrgConfigService>();
            orgConfig.Setup(x => x.GetConfigAsync())
                .ReturnsAsync(new OrgConfigDto
                {
                    Branding = new BrandingDto { InstitutionName = "Govt. Haraganga College" }
                });

            _service = new MemberService(_context, storage.Object, otp.Object, email.Object, user.Object, comm.Object, logger.Object, activity.Object, notify.Object, appSettings, gamification.Object, financials.Object, new Mock<IRealTimeService>().Object, orgConfig.Object, new Mock<ITokenService>().Object);

            if (!_context.ECPeriods.Any(p => p.Title == "Interim Executive Committee"))
            {
                _context.ECPeriods.Add(new ECPeriod { Title = "Interim Executive Committee", StartDate = DateTime.UtcNow.AddYears(-1), IsActive = true });
                _context.SaveChanges();
            }
        }

        [Category("FR-34")]
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
                    PositionId = EcPositionIds.President,
                    StartDate = DateTime.UtcNow,
                    IsCurrent = true
                }
            };

            // Act
            await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            // Assert
            var ecMember = await _context.ECMembers.Include(em => em.ECPeriod).FirstOrDefaultAsync(em => em.MemberId == member.Id);
            ecMember.Should().NotBeNull();
            ecMember!.Position.Should().Be(EcPositionIds.President);
            ecMember.ECPeriod!.Title.Should().Be("Interim Executive Committee");
        }

        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_ShouldEndPreviousRecord_WhenPositionChangesMidTerm()
        {
            // Arrange
            var period = await _context.ECPeriods.FirstOrDefaultAsync(p => p.IsActive);
            period.Should().NotBeNull();

            var member = await CreateAndSaveTestMemberAsync("Tester");

            var oldRecord = new ECMember { MemberId = member.Id, ECPeriodId = period!.Id, Position = EcPositionIds.President, StartDate = DateTime.UtcNow.AddMonths(-2) };
            _context.ECMembers.Add(oldRecord);
            await _context.SaveChangesAsync();

            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>(); // Empty history removes the current role when the logic wipes and replaces it.

            updateDto.ECHistory.Add(new ECHistoryDto
            {
                PeriodTitle = "Interim Executive Committee",
                PositionId = EcPositionIds.President,
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

        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_FallsBackToThePositionKey_WhenNoIdIsSent()
        {
            var member = await CreateAndSaveTestMemberAsync("Tester");
            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto { PeriodTitle = "Interim Executive Committee", Position = "Treasurer", StartDate = DateTime.UtcNow, IsCurrent = true }
            };

            await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            var ecMember = await _context.ECMembers.SingleAsync(em => em.MemberId == member.Id);
            ecMember.Position.Should().Be(EcPositionIds.Treasurer);
        }

        // The member search filter ignores case, so the history sync has to match keys the same way.
        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_PositionKeyFallback_IgnoresCase()
        {
            var member = await CreateAndSaveTestMemberAsync("Tester");
            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto { PeriodTitle = "Interim Executive Committee", Position = "treasurer", StartDate = DateTime.UtcNow, IsCurrent = true }
            };

            await _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            var ecMember = await _context.ECMembers.SingleAsync(em => em.MemberId == member.Id);
            ecMember.Position.Should().Be(EcPositionIds.Treasurer);
        }

        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_RefusesAnUnknownPosition()
        {
            var member = await CreateAndSaveTestMemberAsync("Tester");
            var updateDto = CreateUpdateDto(member);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto { PeriodTitle = "Interim Executive Committee", Position = "ChiefWhip", StartDate = DateTime.UtcNow, IsCurrent = true }
            };

            var act = () => _service.AdminUpdateMemberAsync(member.Id, updateDto, 1, isPrivilegedCaller: true);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*committee position*");
        }

        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_AllowsASecondHolder_WhenThePositionHasASpareSeat()
        {
            var period = await _context.ECPeriods.FirstAsync(p => p.Title == "Interim Executive Committee");
            var position = await _context.EcPositions.FirstAsync(p => p.Id == EcPositionIds.Member1);
            position.SeatCount = 2;
            var first = await CreateAndSaveTestMemberAsync("First", "first@example.com", "01711111111", "1111111111");
            _context.ECMembers.Add(new ECMember { MemberId = first.Id, ECPeriodId = period.Id, Position = position.Id, StartDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var second = await CreateAndSaveTestMemberAsync("Second", "second@example.com", "01722222222", "2222222222");
            var updateDto = CreateUpdateDto(second);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto { PeriodTitle = period.Title, PositionId = position.Id, StartDate = DateTime.UtcNow, IsCurrent = true }
            };

            await _service.AdminUpdateMemberAsync(second.Id, updateDto, 1, isPrivilegedCaller: true);

            (await _context.ECMembers.CountAsync(em => em.Position == position.Id && em.EndDate == null)).Should().Be(2);
        }

        [Category("FR-34")]
        [Test]
        public async Task AdminUpdateMemberAsync_RefusesAHolder_WhenEverySeatIsTaken()
        {
            var period = await _context.ECPeriods.FirstAsync(p => p.Title == "Interim Executive Committee");
            var holder = await CreateAndSaveTestMemberAsync("Holder", "holder@example.com", "01733333333", "3333333333");
            _context.ECMembers.Add(new ECMember { MemberId = holder.Id, ECPeriodId = period.Id, Position = EcPositionIds.Treasurer, StartDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var other = await CreateAndSaveTestMemberAsync("Other", "other@example.com", "01744444444", "4444444444");
            var updateDto = CreateUpdateDto(other);
            updateDto.ECHistory = new List<ECHistoryDto>
            {
                new ECHistoryDto { PeriodTitle = period.Title, PositionId = EcPositionIds.Treasurer, StartDate = DateTime.UtcNow, IsCurrent = true }
            };

            var act = () => _service.AdminUpdateMemberAsync(other.Id, updateDto, 1, isPrivilegedCaller: true);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already has a current holder*");
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
                IsVerified = m.IsVerified,
                AcademicHistory = m.AcademicHistory
                    .Select(a => new AcademicRecordDto
                    {
                        InstitutionName = a.InstitutionName,
                        Degree = a.Degree,
                        Subject = a.Subject,
                        AdmissionYear = a.AdmissionYear,
                        PassingYear = a.PassingYear,
                        IsOrgProfile = a.IsOrgProfile,
                        Result = a.Result
                    })
                    .ToList()
            };
        }
    }
}
