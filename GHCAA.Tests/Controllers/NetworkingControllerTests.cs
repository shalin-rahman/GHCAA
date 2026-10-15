using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

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

        [Category("FR-05")]
        [Test]
        public async Task Search_ReturnsOk()
        {
            var filter = new MemberSearchFilterDto();
            _networkingServiceMock.Setup(x => x.SearchMembersAsync(filter, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new PagedResult<MemberSummaryDto> { Items = new List<MemberSummaryDto>(), TotalItems = 0, TotalPages = 0, Page = 1, PageSize = 20 });
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "Test"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            var result = await _controller.Search(filter, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-05")]
        [Test]
        public async Task Search_Anonymous_UsesPublicListing()
        {
            var filter = new MemberSearchFilterDto();
            _networkingServiceMock.Setup(x => x.SearchPublicDirectoryAsync(filter, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new PagedResult<MemberSummaryDto>());
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            await _controller.Search(filter, CancellationToken.None);

            _networkingServiceMock.Verify(x => x.SearchPublicDirectoryAsync(filter, It.IsAny<CancellationToken>()), Times.Once);
            _networkingServiceMock.Verify(x => x.SearchMembersAsync(It.IsAny<MemberSearchFilterDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-05")]
        [Test]
        public async Task Search_SignedIn_UsesFullSearch()
        {
            var filter = new MemberSearchFilterDto();
            _networkingServiceMock.Setup(x => x.SearchMembersAsync(filter, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new PagedResult<MemberSummaryDto>());
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "Test"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            await _controller.Search(filter, CancellationToken.None);

            _networkingServiceMock.Verify(x => x.SearchMembersAsync(filter, It.IsAny<CancellationToken>()), Times.Once);
            _networkingServiceMock.Verify(x => x.SearchPublicDirectoryAsync(It.IsAny<MemberSearchFilterDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-05")]
        [Test]
        public void GetPublicProfile_IsNotAnonymous()
        {
            var method = typeof(NetworkingController).GetMethod(nameof(NetworkingController.GetPublicProfile))!;

            Assert.That(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true), Is.Empty);
            Assert.That(typeof(NetworkingController).GetCustomAttributes(typeof(AuthorizeAttribute), true), Is.Not.Empty);
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
