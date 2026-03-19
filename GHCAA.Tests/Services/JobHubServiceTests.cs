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

        [SetUp]
        public void Setup()
        {
            _notificationMock = new Mock<INotificationService>();
            _userServiceMock = new Mock<IUserService>();
            _service = new JobHubService(_context, _notificationMock.Object, _userServiceMock.Object);

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

            var dto = new CreateJobDto { Title = "Software Engineer", CompanyName = "Tech Corp", Location = "Dhaka", Description = "Develop software", Requirements = "C# Knowledge", ApplicationEmail = "jobs@tech.com", ApplicationDeadline = DateTime.UtcNow.AddDays(30), Category = Enums.JobCategory.IT };
            var result = await _service.PostJobAsync(dto, member.Id);

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

            _context.JobOpportunities.Add(new JobOpportunity { Title = "Active Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Expired Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(-1), Category = Enums.JobCategory.IT });
            _context.JobOpportunities.Add(new JobOpportunity { Title = "Inactive Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = false, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT });
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

            var job = new JobOpportunity { Title = "Job To Deactivate", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            var result = await _service.DeactivateJobAsync(job.Id);

            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.IsActive.Should().BeFalse();
        }
    }
}
