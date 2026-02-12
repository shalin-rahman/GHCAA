using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class JobHubServiceTests
    {
        private ApplicationDbContext _context = null!;
        private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
        private JobHubService _service = null!;

        [SetUp]
        public void Setup()
        {
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _service = new JobHubService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
        }

        [Test]
        public async Task PostJobAsync_ShouldAddJobAndReturnDto()
        {
            // Arrange
            var member = new Member 
            { 
                FullName = "Recruiter", Email = "r@e.com", NID = "1", FatherName = "F", MotherName = "M", MobileNo = "01",
                PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", 
                EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "I", Designation = "D"
            };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var dto = new CreateJobDto
            {
                Title = "Software Engineer",
                CompanyName = "Tech Corp",
                Location = "Dhaka",
                Description = "Develop software",
                Requirements = "C# Knowledge",
                ApplicationEmail = "jobs@tech.com",
                ApplicationDeadline = DateTime.UtcNow.AddDays(30),
                Category = Enums.JobCategory.IT
            };

            // Act
            var result = await _service.PostJobAsync(dto, member.Id);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Software Engineer");
            result.CompanyName.Should().Be("Tech Corp");
            result.Requirements.Should().Be("C# Knowledge");
            
            var dbJob = await _context.JobOpportunities.FirstOrDefaultAsync(j => j.Title == "Software Engineer");
            dbJob.Should().NotBeNull();
            dbJob!.PostedByMemberId.Should().Be(member.Id);
            dbJob.IsActive.Should().BeTrue();
        }

        [Test]
        public async Task GetActiveJobsAsync_ShouldReturnOnlyActiveAndUnexpiredJobs()
        {
            // Arrange
            var member = new Member { FullName = "M", Email = "m@e.com", NID = "1", MobileNo = "1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            _context.JobOpportunities.Add(new JobOpportunity 
            { 
                Title = "Active Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", 
                PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT 
            });

            _context.JobOpportunities.Add(new JobOpportunity 
            { 
                Title = "Expired Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", 
                PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(-1), Category = Enums.JobCategory.IT 
            });

            _context.JobOpportunities.Add(new JobOpportunity 
            { 
                Title = "Inactive Job", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", 
                PostedByMemberId = member.Id, IsActive = false, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT 
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveJobsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Active Job");
        }

        [Test]
        public async Task DeactivateJobAsync_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var member = new Member { FullName = "M", Email = "m@e.com", NID = "1", MobileNo = "1", FatherName="F", MotherName="M", PresentAddress="A", PermanentAddress="A", EmergencyContactName="E", EmergencyContactRelation="R", EmergencyContactPhone="0", SubjectGroup="S", ProfessionalSector="I", Designation="D" };
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            var job = new JobOpportunity 
            { 
                Title = "Job To Deactivate", Company = "C", Location = "L", Description = "D", Requirements = "R", ContactEmail = "E", 
                PostedByMemberId = member.Id, IsActive = true, ExpiryDate = DateTime.UtcNow.AddDays(10), Category = Enums.JobCategory.IT 
            };
            _context.JobOpportunities.Add(job);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeactivateJobAsync(job.Id);

            // Assert
            result.Should().BeTrue();
            var dbJob = await _context.JobOpportunities.FindAsync(job.Id);
            dbJob!.IsActive.Should().BeFalse();
        }
    }
}
