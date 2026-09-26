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
    public class ElectionsControllerTests : ControllerTestBase
    {
        private Mock<IElectionService> _electionServiceMock;
        private Mock<IElectionDocumentService> _documentServiceMock;
        private ElectionsController _controller;

        [SetUp]
        public void Setup()
        {
            _electionServiceMock = new Mock<IElectionService>();
            _documentServiceMock = new Mock<IElectionDocumentService>();
            _controller = new ElectionsController(_electionServiceMock.Object, _documentServiceMock.Object);
            SetUserContext(_controller, memberId: 10, role: "Member");
        }

        [Test]
        public async Task Vote_ServiceReturnsFalse_Returns400Problem()
        {
            var dto = new CastVoteDto(1, 2, null);
            _electionServiceMock.Setup(x => x.CastVoteAsync(1, 10, dto, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(false);

            var result = await _controller.Vote(1, dto, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("Vote could not be recorded."));
        }
    }
}
