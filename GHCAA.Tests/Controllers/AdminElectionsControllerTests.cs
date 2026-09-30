using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using static GHCAA.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminElectionsControllerTests : ControllerTestBase
    {
        private Mock<IElectionService> _electionServiceMock;
        private Mock<IElectionAccessService> _accessMock;
        private AdminElectionsController _controller;

        [SetUp]
        public void Setup()
        {
            _electionServiceMock = new Mock<IElectionService>();
            _accessMock = new Mock<IElectionAccessService>();
            _controller = new AdminElectionsController(_electionServiceMock.Object, _accessMock.Object);
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

        private static AdminElectionDto Election(int id) => new(id, $"Election {id}", null, ElectionPhase.Announced, null, null, null, null, null, null, null, null, true,
            Array.Empty<AdminElectionPositionDto>(), Array.Empty<AdminElectionCandidateDto>(), 0, false, ElectionTieRule.DrawingLots);

        [Test]
        public async Task List_Official_SeesOnlyTheirElections_WithTheirPermissions()
        {
            SetUserContext(_controller, memberId: null, role: Constants.Roles.ElectionOfficial, userId: 9);
            _electionServiceMock.Setup(x => x.ListAdminElectionsAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new[] { Election(1), Election(2) });
            _accessMock.Setup(x => x.ElectionIdsWithLiveAppointmentAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { 2 });
            _accessMock.Setup(x => x.GetPermissionsAsync(2, 9, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(ElectionPermission.Count | ElectionPermission.Declare);
            _accessMock.Setup(x => x.IsHandedOverAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.List(CancellationToken.None) as OkObjectResult;

            var list = ((IEnumerable<AdminElectionDto>)result!.Value!).ToList();
            Assert.That(list.Select(e => e.Id), Is.EqualTo(new[] { 2 }));
            Assert.That(list[0].MyPermissions, Is.EquivalentTo(new[] { "Count", "Declare" }));
            Assert.That(list[0].AdminHandedOver, Is.True);
        }

        [Test]
        public async Task List_Admin_SeesEveryElection()
        {
            SetUserContext(_controller, memberId: null, role: Constants.Roles.Admin, userId: 1);
            _electionServiceMock.Setup(x => x.ListAdminElectionsAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new[] { Election(1), Election(2) });

            var result = await _controller.List(CancellationToken.None) as OkObjectResult;

            Assert.That(((IEnumerable<AdminElectionDto>)result!.Value!).Count(), Is.EqualTo(2));
            _accessMock.Verify(x => x.ElectionIdsWithLiveAppointmentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
