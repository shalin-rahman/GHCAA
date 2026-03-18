using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class EventsControllerTests
    {
        private Mock<IEventService> _eventServiceMock;
        private EventsController _controller;

        [SetUp]
        public void Setup()
        {
            _eventServiceMock = new Mock<IEventService>();
            _controller = new EventsController(_eventServiceMock.Object);
            
            SetUserContext(1, 10); // Admin 1, Member 10
        }

        private void SetUserContext(int userId, int? memberId, string role = "Admin")
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            if (memberId.HasValue) 
                claims.Add(new Claim("MemberId", memberId.Value.ToString()));

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetActiveEvents_ReturnsOk()
        {
             _eventServiceMock.Setup(x => x.GetActiveEventsAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new List<EventDto>());

            var result = await _controller.GetActiveEvents(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetEventById_ReturnsOk_IfFound()
        {
             _eventServiceMock.Setup(x => x.GetEventByIdAsync(1, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new EventDto { Id = 1 });

            var result = await _controller.GetEventById(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RegisterForEvent_ReturnsOk()
        {
            var dto = new RegisterForEventDto { EventId = 1, PaymentReference = "123" };
            _eventServiceMock.Setup(x => x.RegisterForEventAsync(dto, 10, It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new EventRegistration { Id = 1 });

            var result = await _controller.RegisterForEvent(dto, null, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyRegistrations_ReturnsOk()
        {
            _eventServiceMock.Setup(x => x.GetRegistrationsByMemberAsync(10, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new List<EventRegistration>());

            var result = await _controller.GetMyRegistrations(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyRegistrations_SuperAdmin_ReturnsEmptyList()
        {
            SetUserContext(1, null, "SuperAdmin");
            var result = await _controller.GetMyRegistrations(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Empty);
        }
        
        [Test]
        public async Task GetAllEventsForAdmin_ReturnsOk()
        {
             _eventServiceMock.Setup(x => x.GetAllEventsForAdminAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new List<EventDto>());

            var result = await _controller.GetAllEventsForAdmin(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateEvent_ReturnsCreatedAtAction()
        {
            var dto = new CreateEventDto { Title = "Event" };
            _eventServiceMock.Setup(x => x.CreateEventAsync(dto, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new AlumniEvent { Id = 5 });

            var result = await _controller.CreateEvent(dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task UpdateEvent_ReturnsOk()
        {
            var dto = new UpdateEventDto { Title = "Edit" };
            _eventServiceMock.Setup(x => x.UpdateEventAsync(It.IsAny<UpdateEventDto>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new AlumniEvent { Id = 1 });

            var result = await _controller.UpdateEvent(1, dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteEvent_ReturnsOk_OnSuccess()
        {
            _eventServiceMock.Setup(x => x.DeleteEventAsync(1, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(true);

            var result = await _controller.DeleteEvent(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task ApproveRegistration_ReturnsOk_OnSuccess()
        {
            var dto = new ApproveRegistrationDto { RegistrationId = 1, Approve = true };
            _eventServiceMock.Setup(x => x.ApproveRegistrationAsync(1, 1, true, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(true);

            var result = await _controller.ApproveRegistration(dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task UploadEventLogo_ReturnsOk()
        {
            var logo = new Mock<IFormFile>();
            logo.Setup(x => x.Length).Returns(100);
            logo.Setup(x => x.FileName).Returns("logo.png");
            logo.Setup(x => x.ContentType).Returns("image/png");
            _eventServiceMock.Setup(x => x.UpdateEventLogoAsync(1, It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync("/uploads/logo.png");

            var result = await _controller.UploadEventLogo(1, logo.Object, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
