using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class JobHubControllerTests
    {
        private Mock<IJobHubService> _jobServiceMock;
        private JobHubController _controller;

        [SetUp]
        public void Setup()
        {
            _jobServiceMock = new Mock<IJobHubService>();
            _controller = new JobHubController(_jobServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim("MemberId", "10")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetActiveJobs_ReturnsOk()
        {
            _jobServiceMock.Setup(x => x.GetActiveJobsAsync(null, null, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<JobDto>());

            var result = await _controller.GetActiveJobs(null, null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task PostJob_ReturnsOk()
        {
            var dto = new CreateJobDto { Title = "Test Job" };
            _jobServiceMock.Setup(x => x.PostJobAsync(dto, 10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new JobDto { Id = 1 });

            var result = await _controller.PostJob(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateJob_ReturnsOk_OnSuccess()
        {
            var dto = new CreateJobDto { Title = "Updated" };
            _jobServiceMock.Setup(x => x.UpdateJobAsync(1, dto, 10, false, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

            var result = await _controller.UpdateJob(1, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetJob_ReturnsOk_IfFound()
        {
            _jobServiceMock.Setup(x => x.GetJobByIdAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new JobDto { Id = 1 });

            var result = await _controller.GetJob(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeactivateJob_ReturnsOk_OnSuccess()
        {
            _jobServiceMock.Setup(x => x.GetJobByIdAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new JobDto { Id = 1, PostedByMemberId = 10 });
            _jobServiceMock.Setup(x => x.DeactivateJobAsync(1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

            var result = await _controller.DeactivateJob(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}
