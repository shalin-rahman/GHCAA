using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class ContactControllerTests
    {
        private Mock<IContactService> _contactServiceMock;
        private ContactController _controller;

        [SetUp]
        public void Setup()
        {
            _contactServiceMock = new Mock<IContactService>();
            _controller = new ContactController(_contactServiceMock.Object);
        }

        [Test]
        public async Task Submit_ReturnsOk()
        {
            var dto = new ContactMessageDto { FullName = "Test", Email = "test@test.com", Message = "Hello" };
            _contactServiceMock.Setup(x => x.SubmitMessageAsync(dto, It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            var result = await _controller.Submit(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
