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

        // Only a valid MemberId FK matters for these tests, not membership status/history — matches
        // the 10-field shape TestBase.CreateAndSaveTestMemberAsync fills for the full-detail cases.
        private async Task<Member> CreatePosterMemberAsync(string suffix, string fullName = "M")
        {
            var member = new Member { FullName = fullName, Email = $"{suffix.ToLowerInvariant()}@e.com", NID = suffix, MobileNo = suffix, FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        [Test]
        public async Task PostJobAsync_ShouldAddJobAndReturnDto()
        {
            var member = await CreatePosterMemberAsync("JHR1", "Recruiter");

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
        public async Task UpdateJobAsync_ShouldUpdateAllFields_WhenPoster()
        {
            var poster = await CreatePosterMemberAsync("JHU1", "Poster");
            var job = new JobOpportunity { Title = "Old Title", Company = "Old Co", Location = "Old Loc", Description = "Old Desc", Requirements = "Old Req", ContactEmail = "old@e.com", ApplicationLink = "http://old.example.com", PostedByMemberId = poster.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(5), JobCategory = Enums.JobCategory.IT };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var deadline = DateTime.UtcNow.AddDays(45);
            var dto = new CreateJobDto
            {
                Title = "New Title",
                CompanyName = "New Co",
                Location = "New Loc",
                Description = "New Desc",
                Requirements = "New Req",
                ApplicationEmail = "new@e.com",
                ApplicationLink = "http://new.example.com",
                JobCategory = Enums.JobCategory.Finance,
                ApplicationDeadline = deadline
            };

            var result = await _service.UpdateJobAsync(job.Id, dto, poster.Id, isAdmin: false);

            result.Should().BeTrue();
            var updated = await _context.JobOpportunities.FindAsync(job.Id);
            updated!.Title.Should().Be("New Title");
            updated.Company.Should().Be("New Co");
            updated.Location.Should().Be("New Loc");
            updated.Description.Should().Be("New Desc");
            updated.Requirements.Should().Be("New Req");
            updated.ContactEmail.Should().Be("new@e.com");
            updated.ApplicationLink.Should().Be("http://new.example.com");
            updated.JobCategory.Should().Be(Enums.JobCategory.Finance);
            updated.ExpiryDate.Should().Be(DateTime.SpecifyKind(deadline, DateTimeKind.Utc));
        }

        [Test]
        public async Task UpdateJobAsync_ReturnsFalse_WhenNeitherPosterNorAdmin()
        {
            var poster = await CreatePosterMemberAsync("JHU2", "Poster");
            var otherMember = await CreatePosterMemberAsync("JHU3", "Other");
            var job = new JobOpportunity { Title = "Title", Company = "Co", Location = "Loc", Description = "Desc", Requirements = "Req", ContactEmail = "e@e.com", PostedByMemberId = poster.Id, IsActive = true, JobCategory = Enums.JobCategory.IT };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var dto = new CreateJobDto { Title = "Hijacked", CompanyName = "Co", Location = "Loc", Description = "Desc", Requirements = "Req", JobCategory = Enums.JobCategory.IT };

            var result = await _service.UpdateJobAsync(job.Id, dto, otherMember.Id, isAdmin: false);

            result.Should().BeFalse();
            var unchanged = await _context.JobOpportunities.FindAsync(job.Id);
            unchanged!.Title.Should().Be("Title");
        }

        [Test]
        public async Task GetActiveJobsAsync_ShouldReturnOnlyActiveAndUnexpiredJobs()
        {
            var member = await CreatePosterMemberAsync("JHM1");

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
            var member = await CreatePosterMemberAsync("JHD1");

            var job = new JobOpportunity { Title = "Job To Deactivate", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), JobCategory = Enums.JobCategory.IT };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.DeactivateJobAsync(job.Id);

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.IsActive.Should().BeFalse();
        }

        [Category("FR-54")]
        [Test]
        public async Task PostJobAsync_NonAdmin_SetsStatusPending_AndNotifiesAdmins()
        {
            var member = await CreatePosterMemberAsync("JHP1", "Poster");

            var dto = new CreateJobDto { Title = "Pending Job", CompanyName = "Tech Corp", Location = "Dhaka", Description = "D", Requirements = "R", JobCategory = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id, isAdmin: false);

            result.Status.Should().Be(Enums.SubmissionStatus.Pending);
            _adminNotificationMock.Verify(x => x.NotifyPendingApprovalAsync("Job", "Pending Job", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _notificationMock.Verify(x => x.CreateNotificationAsync(member.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-54")]
        [Test]
        public async Task PostJobAsync_Admin_SetsStatusApproved_AndNotifiesPoster()
        {
            var member = await CreatePosterMemberAsync("JHA1", "AdminPoster");

            var dto = new CreateJobDto { Title = "Admin Job", CompanyName = "Tech Corp", Location = "Dhaka", Description = "D", Requirements = "R", JobCategory = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id, isAdmin: true);

            result.Status.Should().Be(Enums.SubmissionStatus.Approved);
            _adminNotificationMock.Verify(x => x.NotifyPendingApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-54")]
        [Test]
        public async Task ApproveJobAsync_SetsStatusApproved_AndNotifiesPoster()
        {
            var member = await CreatePosterMemberAsync("JHAA1");

            var job = new JobOpportunity { Title = "Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Pending };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.ApproveJobAsync(job.Id);

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.Status.Should().Be(Enums.SubmissionStatus.Approved);
            _notificationMock.Verify(x => x.CreateNotificationAsync(member.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-54")]
        [Test]
        public async Task RejectJobAsync_SetsStatusRejected_AndDeactivates()
        {
            var member = await CreatePosterMemberAsync("JHRJ1");

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

        [Category("FR-54")]
        [Test]
        public async Task GetPendingJobsAsync_ReturnsOnlyPendingJobs()
        {
            var member = await CreatePosterMemberAsync("JHGP1");

            _context.JobOpportunities.Add(new JobOpportunity { Title = "Pending", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Pending });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Approved", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, JobCategory = Enums.JobCategory.IT, Status = Enums.SubmissionStatus.Approved });
            await _context.SaveChangesAsync();

            var result = await _service.GetPendingJobsAsync();

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Pending");
        }
    }
}
