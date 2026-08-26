using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class JobHubServiceTests : TestBase
    {
        private JobHubService _service = null!;
        private Mock<INotificationService> _notificationMock = null!;
        private Mock<IUserService> _userServiceMock = null!;
        private Mock<IAdminNotificationService> _adminNotificationMock = null!;

        [SetUp]
        public void Setup()
        {
            _notificationMock = new Mock<INotificationService>();
            _userServiceMock = new Mock<IUserService>();
            _adminNotificationMock = new Mock<IAdminNotificationService>();
            _service = new JobHubService(_context, _notificationMock.Object, _userServiceMock.Object, _adminNotificationMock.Object);

            // Clear seed data so count assertions are deterministic
            _context.JobOpportunities.RemoveRange(_context.JobOpportunities);
            _context.SaveChanges();
        }

        [Test]
        public async Task PostJobAsync_ShouldAddJobAndReturnDto()
        {
            var member = new Member { FullName = "Recruiter", Email = "jhr@e.com", NID = "JHR1", FatherName = "F", MotherName = "M", MobileNo = "JHR1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var dto = new CreateJobDto { Title = "Software Engineer", CompanyName = "Tech Corp", Location = "Dhaka", Description = "Develop software", Requirements = "C# Knowledge", ApplicationEmail = "jobs@tech.com", ApplicationDeadline = DateTime.UtcNow.AddDays(30), JobCategory = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id, isAdmin: false);

            result.Should().NotBeNull();
            result.Title.Should().Be("Software Engineer");
            result.CompanyName.Should().Be("Tech Corp");

            var dbJob = await _context.JobOpportunities.FirstOrDefaultAsync(j => j.Title == "Software Engineer");
            dbJob.Should().NotBeNull();
            dbJob!.PostedByMemberId.Should().Be(member.Id);
            dbJob.IsActive.Should().BeTrue();
        }

        [Test]
        public async Task GetActiveJobsAsync_ShouldReturnOnlyActiveAndUnexpiredJobs()
        {
            var member = new Member { FullName = "M", Email = "jhm@e.com", NID = "JHM1", MobileNo = "JHM1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            _context.JobOpportunities.Add(new JobOpportunity { Title = "Active Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), JobCategory = Enums.JobCategory.IT });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Expired Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(-1), JobCategory = Enums.JobCategory.IT });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Inactive Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = false, ExpiryDate = DateTime.UtcNow.AddDays(10), JobCategory = Enums.JobCategory.IT });
            await _context.SaveChangesAsync();

            var result = await _service.GetActiveJobsAsync();

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Active Job");
        }

        [Test]
        public async Task DeactivateJobAsync_ShouldSetIsActiveToFalse()
        {
            var member = new Member { FullName = "M", Email = "jhd@e.com", NID = "JHD1", MobileNo = "JHD1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var job = new JobOpportunity { Title = "Job To Deactivate", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), JobCategory = Enums.JobCategory.IT };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.DeactivateJobAsync(job.Id);

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.IsActive.Should().BeFalse();
        }

        [Test]
        public async Task PostJobAsync_NonAdmin_SetsStatusPending_AndNotifiesAdmins()
        {
            var member = new Member { FullName = "Poster", Email = "jhp@e.com", NID = "JHP1", FatherName = "F", MotherName = "M", MobileNo = "JHP1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var dto = new CreateJobDto { Title = "Pending Job", CompanyName = "Tech Corp", Location = "Dhaka", Description = "D", Requirements = "R", JobCategory = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id, isAdmin: false);

            result.Status.Should().Be(Enums.SubmissionStatus.Pending);
            _adminNotificationMock.Verify(x => x.NotifyPendingApprovalAsync("Job", "Pending Job", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationMock.Verify(x => x.CreateNotificationAsync(member.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task PostJobAsync_Admin_SetsStatusApproved_AndNotifiesPoster()
        {
            var member = new Member { FullName = "AdminPoster", Email = "jha@e.com", NID = "JHA1", FatherName = "F", MotherName = "M", MobileNo = "JHA1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var dto = new CreateJobDto { Title = "Admin Job", CompanyName = "Tech Corp", Location = "Dhaka", Description = "D", Requirements = "R", JobCategory = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id, isAdmin: true);

            result.Status.Should().Be(Enums.SubmissionStatus.Approved);
            _adminNotificationMock.Verify(x => x.NotifyPendingApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ApproveJobAsync_SetsStatusApproved_AndNotifiesPoster()
        {
            var member = new Member { FullName = "M", Email = "jhaa@e.com", NID = "JHAA1", MobileNo = "JHAA1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var job = new JobOpportunity { Title = "Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Pending };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.ApproveJobAsync(job.Id);

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.Status.Should().Be(Enums.SubmissionStatus.Approved);
            _notificationMock.Verify(x => x.CreateNotificationAsync(member.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RejectJobAsync_SetsStatusRejected_AndDeactivates()
        {
            var member = new Member { FullName = "M", Email = "jhrj@e.com", NID = "JHRJ1", MobileNo = "JHRJ1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var job = new JobOpportunity { Title = "Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Pending };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.RejectJobAsync(job.Id, "Not relevant");

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.Status.Should().Be(Enums.SubmissionStatus.Rejected);
            dbJob.RejectionReason.Should().Be("Not relevant");
            dbJob.IsActive.Should().BeFalse();
        }

        [Test]
        public async Task GetPendingJobsAsync_ReturnsOnlyPendingJobs()
        {
            var member = new Member { FullName = "M", Email = "jhgp@e.com", NID = "JHGP1", MobileNo = "JHGP1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            _context.JobOpportunities.Add(new JobOpportunity { Title = "Pending", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Pending });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Approved", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Approved });
            await _context.SaveChangesAsync();

            var result = await _service.GetPendingJobsAsync();

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Pending");
        }
    }
}
