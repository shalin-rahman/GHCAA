using FluentAssertions;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class GovernanceControllerTests : TestBase
    {
        private GovernanceService _service = null!;
        private GovernanceController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new GovernanceService(_context, new Mock<INotificationService>().Object);
            _controller = new GovernanceController(_service);
        }

        // The public list feeds pickers on every client, so a retired position must not come back.
        [Category("FR-34")]
        [Test]
        public async Task GetPositions_LeavesOutARetiredPosition()
        {
            await _service.SetPositionActiveAsync(EcPositionIds.LawSecretary, false);

            var result = await _controller.GetPositions(CancellationToken.None);

            var positions = result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<IReadOnlyList<EcPositionDto>>().Subject;
            positions.Should().NotBeEmpty().And.OnlyContain(p => p.IsActive);
            positions.Should().NotContain(p => p.Id == EcPositionIds.LawSecretary);
        }
    }
}
