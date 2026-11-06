using System.Reflection;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class CampaignsControllerTests : ControllerTestBase
    {
        private Mock<ICampaignService> _service = null!;
        private CampaignsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<ICampaignService>();
            _controller = new CampaignsController(_service.Object);
            SetMemberContext(_controller, 100);
        }

        [Category("FR-044")]
        [Test]
        public async Task GetBySlug_Unknown_ReturnsNotFound()
        {
            _service.Setup(s => s.GetCampaignBySlugAsync("nope", It.IsAny<CancellationToken>())).ReturnsAsync((CampaignDto?)null);

            var result = await _controller.GetBySlug("nope", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-045")]
        [Test]
        public async Task CreatePledge_SignedInMember_LinksMemberId()
        {
            var dto = new CreatePledgeDto { Amount = 500 };
            _service.Setup(s => s.CreatePledgeAsync("library", dto, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CampaignPledgeDto { Id = 1, MemberId = 100 });

            var result = await _controller.CreatePledge("library", dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-045")]
        [Test]
        public async Task CreatePledge_Guest_PassesNullMemberId()
        {
            SetUserContext(_controller, memberId: null, role: "Guest", userId: null);
            var dto = new CreatePledgeDto { Amount = 500 };
            _service.Setup(s => s.CreatePledgeAsync("library", dto, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CampaignPledgeDto { Id = 2 });

            var result = await _controller.CreatePledge("library", dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _service.Verify(s => s.CreatePledgeAsync("library", dto, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-045")]
        [Test]
        public async Task CreatePledge_UnknownCampaign_Returns404()
        {
            _service.Setup(s => s.CreatePledgeAsync("gone", It.IsAny<CreatePledgeDto>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Campaign not found."));

            var result = await _controller.CreatePledge("gone", new CreatePledgeDto { Amount = 1 }, CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(404));
        }

        [Category("FR-045")]
        [Test]
        public async Task CreatePledge_InvalidRequest_Returns400()
        {
            _service.Setup(s => s.CreatePledgeAsync("closed", It.IsAny<CreatePledgeDto>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidRequestException("Campaign is closed."));

            var result = await _controller.CreatePledge("closed", new CreatePledgeDto { Amount = 1 }, CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(400));
        }

        [Category("FR-046")]
        [Test]
        public async Task GetMyPledges_WithoutMemberClaim_ReturnsUnauthorized()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 1);

            var result = await _controller.GetMyPledges(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Category("FR-048")]
        [Test]
        public async Task ConfirmReceipt_UnknownPledge_ReturnsNotFound()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 5);
            _service.Setup(s => s.ConfirmPledgeReceiptAsync(It.IsAny<ConfirmPledgeReceiptDto>(), 5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.ConfirmReceipt(new ConfirmPledgeReceiptDto(), CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-047")]
        [TestCase(nameof(CampaignsController.GetAllForAdmin))]
        [TestCase(nameof(CampaignsController.Create))]
        [TestCase(nameof(CampaignsController.Update))]
        [TestCase(nameof(CampaignsController.GetPledgesForAdmin))]
        [TestCase(nameof(CampaignsController.ConfirmReceipt))]
        [TestCase(nameof(CampaignsController.GetTiers))]
        [TestCase(nameof(CampaignsController.CreateTier))]
        public void AdminActions_RequireAdminOnlyPolicy(string action)
        {
            var attr = typeof(CampaignsController).GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.AdminOnly));
        }

        [Category("FR-050")]
        [TestCase(nameof(CampaignsController.GetPublicCampaigns))]
        [TestCase(nameof(CampaignsController.GetBySlug))]
        [TestCase(nameof(CampaignsController.GetHonourRoll))]
        [TestCase(nameof(CampaignsController.CreatePledge))]
        public void PublicActions_AllowAnonymous(string action)
        {
            Assert.That(typeof(CampaignsController).GetMethod(action)!.GetCustomAttribute<AllowAnonymousAttribute>(), Is.Not.Null);
        }
    }
}
