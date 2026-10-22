using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Controllers
{
    // 37.14s. Public reads of published election content, and admin edits.
    [TestFixture]
    [Category("FR-25")]
    public class ElectionContentControllerTests : ControllerTestBase
    {
        private Mock<IElectionContentService> _serviceMock = null!;
        private ElectionContentController _controller = null!;

        private static UpsertElectionContentDto Form() =>
            new() { Kind = ElectionContentKind.Form, Key = "ER-90", Title = "Extra", Body = "{}" };

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IElectionContentService>();
            _controller = new ElectionContentController(_serviceMock.Object);
            SetUserContext(_controller, null, "Admin", 1);
        }

        [Test]
        public async Task GetPublished_PassesTheKind()
        {
            _serviceMock.Setup(x => x.GetPublishedAsync((ElectionContentKind?)ElectionContentKind.Page, It.IsAny<CancellationToken>()))
                        .ReturnsAsync([new ElectionContentDto { Key = "regulations", Title = "Regulations" }]);

            var result = await _controller.GetPublished(ElectionContentKind.Page, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPublishedByKey_Missing_Returns404()
        {
            _serviceMock.Setup(x => x.GetPublishedAsync(ElectionContentKind.Form, "ER-99", It.IsAny<CancellationToken>()))
                        .ReturnsAsync((ElectionContentDto?)null);

            Assert.That(await _controller.GetPublishedByKey(ElectionContentKind.Form, "ER-99", CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetById_Missing_Returns404()
        {
            _serviceMock.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((ElectionContentDto?)null);

            Assert.That(await _controller.GetById(5, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_ReturnsCreated_AndPassesAdminId()
        {
            _serviceMock.Setup(x => x.CreateAsync(It.IsAny<UpsertElectionContentDto>(), 1, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((new ElectionContentDto { Id = 3, Key = "ER-90", Title = "Extra" }, (string?)null));

            Assert.That(await _controller.Create(Form(), CancellationToken.None), Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task Create_ServiceError_Returns400Problem()
        {
            _serviceMock.Setup(x => x.CreateAsync(It.IsAny<UpsertElectionContentDto>(), 1, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(((ElectionContentDto?)null, "A form with key ER-90 already exists."));

            var result = await _controller.Create(Form(), CancellationToken.None) as ObjectResult;

            Assert.That(result!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)result.Value!).Detail, Is.EqualTo("A form with key ER-90 already exists."));
        }

        [Test]
        public async Task Create_NoUserId_ReturnsUnauthorized()
        {
            SetUserContext(_controller, null, "Admin", null);

            Assert.That(await _controller.Create(Form(), CancellationToken.None), Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Update_UnknownId_Returns404()
        {
            _serviceMock.Setup(x => x.UpdateAsync(8, It.IsAny<UpsertElectionContentDto>(), 1, It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new KeyNotFoundException());

            Assert.That(await _controller.Update(8, Form(), CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Update_Valid_ReturnsOk()
        {
            _serviceMock.Setup(x => x.UpdateAsync(8, It.IsAny<UpsertElectionContentDto>(), 1, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((new ElectionContentDto { Id = 8, Key = "ER-90", Title = "Extra" }, (string?)null));

            Assert.That(await _controller.Update(8, Form(), CancellationToken.None), Is.InstanceOf<OkObjectResult>());
        }
    }
}
