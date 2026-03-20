using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Workflows
{
    [TestFixture]
    public class WorkflowTests : TestBase
    {
        private MemberService _memberService;
        private EventService _eventService;
        private FinancialService _financialService;
        private Mock<IEmailService> _emailMock;
        private Mock<IFileStorageService> _storageMock;
        private Mock<IFileUploadRepository> _fileRepoMock;
        private Mock<IOtpService> _otpMock;
        private Mock<INotificationService> _notificationMock;
        private Mock<IActivityService> _activityMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ICommunicationService> _commMock;

        [SetUp]
        public void WorkflowSetup()
        {
            _emailMock = new Mock<IEmailService>();
            _storageMock = new Mock<IFileStorageService>();
            _fileRepoMock = new Mock<IFileUploadRepository>();
            _otpMock = new Mock<IOtpService>();
            _notificationMock = new Mock<INotificationService>();
            _activityMock = new Mock<IActivityService>();
            _userServiceMock = new Mock<IUserService>();
            _commMock = new Mock<ICommunicationService>();

            var loggerMock = new Mock<ILogger<MemberService>>();

            var configMock = new Mock<IConfiguration>();

            _memberService = new MemberService(
                _context, 
                _storageMock.Object,
                _fileRepoMock.Object,
                _otpMock.Object,
                _emailMock.Object,
                _userServiceMock.Object, 
                _commMock.Object, 
                loggerMock.Object,
                _activityMock.Object,
                _notificationMock.Object,
                configMock.Object);

            _eventService = new EventService(_context, _commMock.Object, _storageMock.Object);
            _financialService = new FinancialService(_context, _commMock.Object, _notificationMock.Object);
        }

        [Test]
        public async Task Registration_To_EventApproval_Workflow()
        {
            // 1. Member Registers
            var regDto = new MemberRegistrationDto
            {
                FullName = "Workflow User",
                Email = "workflow@example.com",
                MobileNo = "01711111111",
                NID = "1234567890",
                Gender = GHCAA.Domain.Enums.Gender.Male,
                BloodGroup = GHCAA.Domain.Enums.BloodGroup.APositive,
                FatherName = "Father",
                MotherName = "Mother",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                EmergencyContactName = "Emergency",
                EmergencyContactRelation = "Sibling",
                EmergencyContactPhone = "01700000000",
                PresentAddress = "Dhaka",
                PermanentAddress = "Dhaka",
                AcademicHistory = new List<AcademicRecordDto>
                {
                    new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2020, IsGHC = true }
                }
            };

            var regId = await _memberService.RegisterAsync(regDto, null, null, null, CancellationToken.None);
            Assert.That(regId, Is.GreaterThan(0));

            var member = _context.Members.First(m => m.Email == "workflow@example.com");
            Assert.That(member.Status, Is.EqualTo(Enums.MembershipStatus.Applied));

            // 2. Admin Approves Member
            var approveResult = await _memberService.ApproveMemberAsync(member.Id, 1, CancellationToken.None);
            Assert.That(approveResult.MembershipNumber, Is.Not.Null);
            
            _context.Entry(member).Reload();
            Assert.That(member.Status, Is.EqualTo(Enums.MembershipStatus.Active));

            // 3. Admin Creates Event
            var eventDto = new CreateEventDto
            {
                Title = "Annual Picnic",
                Description = "A fun day out for all alumni",
                Location = "Dhaka City Park",
                Date = DateTime.UtcNow.AddDays(30),
                RegistrationFee = 500,
                IsActive = true
            };
            var alumniEvent = await _eventService.CreateEventAsync(eventDto, CancellationToken.None);
            Assert.That(alumniEvent.Id, Is.GreaterThan(0));

            // 4. Member Registers for Event
            var eventRegDto = new RegisterForEventDto
            {
                EventId = alumniEvent.Id,
                PaymentMethod = Enums.PaymentMethod.BKash,
                PaymentReference = "TRX12345"
            };
            var eventReg = await _eventService.RegisterForEventAsync(eventRegDto, member.Id, null, CancellationToken.None);
            Assert.That(eventReg.Status, Is.EqualTo(Enums.EventRegistrationStatus.Pending));

            // 5. Admin Approves Event Registration
            var approveReg = await _eventService.ApproveRegistrationAsync(eventReg.Id, 1, true, CancellationToken.None);
            Assert.That(approveReg, Is.True);

            var finalReg = _context.EventRegistrations.Find(eventReg.Id);
            Assert.That(finalReg.Status, Is.EqualTo(Enums.EventRegistrationStatus.Approved));
        }
    }
}
