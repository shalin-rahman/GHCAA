using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using static GHCAA.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // Remove/Cancel were the two mutation actions still untested (47.13.6). TODO.md's
    // "CancelRequest"/"UnlinkMember" names don't exist on this controller — Remove and
    // Cancel are the only DELETE/POST actions here beyond Send/Respond.
    // Send/Respond added in 47.13.7's re-audit — they'd been missed by both the original
    // list and 47.13.6's pass since they don't read as CRUD verbs.
    [TestFixture]
    public class FamilyLinkControllerTests : ControllerTestBase
    {
        private Mock<IFamilyLinkService> _familyLinkServiceMock;
        private Mock<IFamilyService> _familyServiceMock;
        private FamilyLinkController _controller;

        [SetUp]
        public void Setup()
        {
            _familyLinkServiceMock = new Mock<IFamilyLinkService>();
            _familyServiceMock = new Mock<IFamilyService>();
            _controller = new FamilyLinkController(_familyLinkServiceMock.Object, _familyServiceMock.Object);
            SetMemberContext(_controller, 10);
        }

        [Test]
        public async Task Remove_ReturnsOk_OnSuccess()
        {
            _familyLinkServiceMock.Setup(x => x.RemoveLinkAsync(10, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.Remove(5);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Remove_ReturnsNotFound_WhenLinkMissing()
        {
            _familyLinkServiceMock.Setup(x => x.RemoveLinkAsync(10, 99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Remove(99);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Cancel_ReturnsOk_OnSuccess()
        {
            _familyLinkServiceMock.Setup(x => x.CancelAsync(10, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.Cancel(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Cancel_ReturnsNotFound_WhenRequestMissing()
        {
            _familyLinkServiceMock.Setup(x => x.CancelAsync(10, 99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Cancel(99, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Send_ReturnsOk_OnSuccess()
        {
            var dto = new SendFamilyLinkDto { TargetMembershipNumber = "GHC-2020-001", Relationship = RelationshipType.Sibling };
            var response = new FamilyLinkRequestDto { Id = 1, RequesterId = 10 };
            _familyLinkServiceMock.Setup(x => x.SendRequestAsync(10, dto, It.IsAny<CancellationToken>())).ReturnsAsync(response);

            var result = await _controller.Send(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(response));
        }

        [Test]
        public async Task Send_ReturnsNotFound_WhenTargetMemberMissing()
        {
            var dto = new SendFamilyLinkDto { TargetMembershipNumber = "NO-SUCH-MEMBER", Relationship = RelationshipType.Sibling };
            _familyLinkServiceMock.Setup(x => x.SendRequestAsync(10, dto, It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new KeyNotFoundException("Member not found."));

            var result = await _controller.Send(dto, CancellationToken.None);

            var problem = result as ObjectResult;
            Assert.That(problem?.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        }

        [Test]
        public async Task Respond_ReturnsOk_OnSuccess()
        {
            var dto = new RespondFamilyLinkDto { RequestId = 5, Approve = true };
            _familyLinkServiceMock.Setup(x => x.RespondAsync(10, dto, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.Respond(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Respond_ReturnsNotFound_WhenRequestMissing()
        {
            var dto = new RespondFamilyLinkDto { RequestId = 99, Approve = true };
            _familyLinkServiceMock.Setup(x => x.RespondAsync(10, dto, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Respond(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
