using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminElectionsControllerTests : ControllerTestBase
    {
        private Mock<IElectionService> _electionServiceMock;
        private AdminElectionsController _controller;

        [SetUp]
        public void Setup()
        {
            _electionServiceMock = new Mock<IElectionService>();
            _controller = new AdminElectionsController(_electionServiceMock.Object);
            SetUserContext(_controller, memberId: 1, role: "Admin");
        }

        [Test]
        public async Task Publish_ElectionNotReady_Returns400Problem()
        {
            _electionServiceMock.Setup(x => x.SetPhaseAsync(1, It.IsAny<GHCAA.Domain.Enums.ElectionPhase>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(false);

            var result = await _controller.Publish(1, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("Election is not ready to publish."));
        }

        [Test]
        public async Task RemoveCandidate_HasVotes_Returns409Problem()
        {
            _electionServiceMock.Setup(x => x.RemoveCandidateAsync(1, 5, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((false, "has-votes"));

            var result = await _controller.RemoveCandidate(1, 5, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(409));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("This candidate already has votes recorded and cannot be removed."));
        }
    }
}
