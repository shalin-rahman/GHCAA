using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IIDCardService> _idCardServiceMock;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _idCardServiceMock = new Mock<IIDCardService>();

            _controller = new AdminController(_memberServiceMock.Object, _idCardServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Role, "SuperAdmin")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetAllMembers_ReturnsOk_WithData()
        {
            var fakeResult = new { TotalItems = 1, Items = new object[] { } };
            _memberServiceMock.Setup(x => x.GetAllMembersAsync(1, 10, "", "Applied", false, true, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(fakeResult);

            var result = await _controller.GetAllMembers(1, 10, "", "Applied", false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(fakeResult));
        }

        [Test]
        public async Task ApproveMember_ReturnsOk_OnSuccess()
        {
            var dto = new ApproveMemberDto { ApprovedByAdminId = 1 };
            _memberServiceMock.Setup(x => x.ApproveMemberAsync(100, 1, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new ApproveMemberResultDto { MembershipNumber = "GHC-2023-0001", DefaultPassword = "GHC" });

            var result = await _controller.ApproveMember(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RejectMember_ReturnsOk_OnSuccess()
        {
            var dto = new RejectMemberDto { RejectedByAdminId = 1, Reason = "Invalid Data" };
            _memberServiceMock.Setup(x => x.RejectMemberAsync(100, 1, "Invalid Data", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.RejectMember(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ArchiveMember_ReturnsOk_OnSuccess()
        {
            _memberServiceMock.Setup(x => x.ArchiveMemberAsync(100, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.ArchiveMember(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ReactivateMember_ReturnsOk_OnSuccess()
        {
            _memberServiceMock.Setup(x => x.ReactivateMemberAsync(100, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.ReactivateMember(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ResetPasswordAdmin_ReturnsOk_OnSuccess()
        {
            _memberServiceMock.Setup(x => x.SendAdminPasswordResetLinkAsync(100, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.ResetPasswordAdmin(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateMemberAdmin_ReturnsOk_OnSuccess()
        {
            var dto = new AdminMemberUpdateDto { FullName = "Updated Name" };
            _memberServiceMock.Setup(x => x.AdminUpdateMemberAsync(100, dto, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.UpdateMemberAdmin(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
