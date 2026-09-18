using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerTests : ControllerTestBase
    {
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IIDCardService> _idCardServiceMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _idCardServiceMock = new Mock<IIDCardService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Ok());

            _controller = new AdminController(_memberServiceMock.Object, _idCardServiceMock.Object, _fileValidationServiceMock.Object, Mock.Of<ILogger<AdminController>>());

            SetSuperAdminContext(_controller, 1);
        }

        [Test]
        public async Task GetAllMembers_ReturnsOk_WithData()
        {
            var fakeResult = new { TotalItems = 1, Items = new object[] { } };
            _memberServiceMock.Setup(x => x.GetAllMembersAsync(1, 10, "", "Applied", "all", "all", false, true, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(fakeResult);

            var result = await _controller.GetAllMembers(1, 10, "", "Applied", "all", "all", false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(fakeResult));
        }

        [Test]
        public async Task ApproveMember_ReturnsOk_OnSuccess()
        {
            // Admin identity (1) comes from the JWT MemberId claim set by SetSuperAdminContext, not the DTO.
            var dto = new ApproveMemberDto();
            _memberServiceMock.Setup(x => x.ApproveMemberAsync(100, 1, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new ApproveMemberResultDto { MembershipNumber = "GHC-2023-0001", DefaultPassword = "GHC" });

            var result = await _controller.ApproveMember(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RejectMember_ReturnsOk_OnSuccess()
        {
            var dto = new RejectMemberDto { Reason = "Invalid Data" };
            _memberServiceMock.Setup(x => x.RejectMemberAsync(100, 1, "Invalid Data", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.RejectMember(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RevertMemberApproval_ReturnsOk_OnSuccess()
        {
            _memberServiceMock.Setup(x => x.RevertMemberApprovalAsync(100, 1, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.RevertMemberApproval(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RevertMemberApproval_ReturnsNotFound_WhenMemberMissing()
        {
            _memberServiceMock.Setup(x => x.RevertMemberApprovalAsync(100, 1, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(false);

            var result = await _controller.RevertMemberApproval(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task RevertMemberApproval_ReturnsBadRequest_WhenMemberNotActive()
        {
            _memberServiceMock.Setup(x => x.RevertMemberApprovalAsync(100, 1, It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new InvalidOperationException("Only active members can have approval reverted."));

            var result = await _controller.RevertMemberApproval(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var problemResult = result as ObjectResult;
            Assert.That(problemResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
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
            _memberServiceMock.Setup(x => x.SendAdminPasswordResetLinkAsync(100, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync((true, "http://reset"));

            var result = await _controller.ResetPasswordAdmin(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateMemberAdmin_ReturnsOk_OnSuccess()
        {
            var dto = new AdminMemberUpdateDto { FullName = "Updated Name" };
            _memberServiceMock.Setup(x => x.AdminUpdateMemberAsync(100, dto, 1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.UpdateMemberAdmin(100, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task SyncMembers_ReturnsOk_WithCount()
        {
            _memberServiceMock.Setup(x => x.SyncAlumniAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);

            var result = await _controller.SyncMembers(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void SyncMembers_PropagatesException_WhenImportSourceUnreachable()
        {
            _memberServiceMock.Setup(x => x.SyncAlumniAsync(It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new InvalidOperationException("Import source unreachable."));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.SyncMembers(CancellationToken.None));
        }

        [Test]
        public async Task BulkArchiveInactive_ReturnsOk_WithCount()
        {
            _memberServiceMock.Setup(x => x.BulkArchiveInactiveMembersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);

            var result = await _controller.BulkArchiveInactive(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void BulkArchiveInactive_PropagatesException_WhenServiceFails()
        {
            _memberServiceMock.Setup(x => x.BulkArchiveInactiveMembersAsync(It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new InvalidOperationException("Bulk archive failed."));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.BulkArchiveInactive(CancellationToken.None));
        }

        [Test]
        public async Task RestoreMember_ReturnsOk_OnSuccess()
        {
            _memberServiceMock.Setup(x => x.RestoreMemberAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.RestoreMember(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RestoreMember_ReturnsNotFound_WhenMemberMissing()
        {
            _memberServiceMock.Setup(x => x.RestoreMemberAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.RestoreMember(999, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateMemberPhoto_ReturnsOk_OnSuccess()
        {
            var photo = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);
            _memberServiceMock.Setup(x => x.UpdateMemberPhotoAsync(100, It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync("/uploads/members/100/photo.jpg");

            var result = await _controller.UpdateMemberPhoto(100, photo, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateMemberPhoto_ReturnsBadRequest_WhenValidationFails()
        {
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Fail("File too large."));
            var photo = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);

            var result = await _controller.UpdateMemberPhoto(100, photo, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateMemberSignature_ReturnsOk_OnSuccess()
        {
            var signature = Mock.Of<IFormFile>(f => f.FileName == "signature.png" && f.Length == 512);
            _memberServiceMock.Setup(x => x.UpdateMemberSignatureAsync(100, It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync("/uploads/members/100/signature.png");

            var result = await _controller.UpdateMemberSignature(100, signature, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateMemberSignature_ReturnsBadRequest_WhenValidationFails()
        {
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Fail("Unsupported format."));
            var signature = Mock.Of<IFormFile>(f => f.FileName == "signature.png" && f.Length == 512);

            var result = await _controller.UpdateMemberSignature(100, signature, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task UpdateMemberDocuments_ReturnsOk_OnSuccess()
        {
            var certificate = Mock.Of<IFormFile>(f => f.FileName == "cert.pdf" && f.Length == 2048);
            _memberServiceMock.Setup(x => x.UpdateMemberDocumentsAsync(100, It.IsAny<UploadedFileDto>(), null, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.UpdateMemberDocuments(100, certificate, null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateMemberDocuments_ReturnsNotFound_WhenMemberMissing()
        {
            var certificate = Mock.Of<IFormFile>(f => f.FileName == "cert.pdf" && f.Length == 2048);
            _memberServiceMock.Setup(x => x.UpdateMemberDocumentsAsync(999, It.IsAny<UploadedFileDto>(), null, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(false);

            var result = await _controller.UpdateMemberDocuments(999, certificate, null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}
