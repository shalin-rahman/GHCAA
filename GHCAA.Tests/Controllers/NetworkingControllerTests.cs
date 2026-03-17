using System.Collections.Generic;
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
    public class NetworkingControllerTests
    {
        private Mock<INetworkingService> _networkingServiceMock;
        private NetworkingController _controller;

        [SetUp]
        public void Setup()
        {
            _networkingServiceMock = new Mock<INetworkingService>();
            _controller = new NetworkingController(_networkingServiceMock.Object);
        }

        [Test]
        public async Task Search_ReturnsOk()
        {
            var filter = new MemberSearchFilterDto();
            _networkingServiceMock.Setup(x => x.SearchMembersAsync(filter, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new PagedResult<MemberSummaryDto> { Items = new List<MemberSummaryDto>(), TotalItems = 0, TotalPages = 0, Page = 1, PageSize = 20 });

            var result = await _controller.Search(filter, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPublicProfile_ReturnsOk_IfFound()
        {
            _networkingServiceMock.Setup(x => x.GetMemberProfileAsync(1, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new MemberProfileDto { Id = 1 });

            var result = await _controller.GetPublicProfile(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetCommittee_ReturnsOk()
        {
            _networkingServiceMock.Setup(x => x.GetExecutiveCommitteeAsync(null, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new List<MemberSummaryDto>());

            var result = await _controller.GetExecutiveCommittee(null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPeriods_ReturnsOk()
        {
            _networkingServiceMock.Setup(x => x.GetECPeriodsAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new List<object>());

            var result = await _controller.GetPeriods(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetLatestUpdates_ReturnsOk()
        {
            _networkingServiceMock.Setup(x => x.GetLatestAlumniUpdatesAsync(10, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new List<MemberSummaryDto>());

            var result = await _controller.GetLatestUpdates(10, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
