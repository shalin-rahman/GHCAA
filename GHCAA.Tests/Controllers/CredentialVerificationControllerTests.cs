using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class CredentialVerificationControllerTests
    {
        [Test]
        public async Task Verify_ReturnsOkForRevokedCredential()
        {
            var service = new Mock<IIDCardService>();
            service.Setup(x => x.VerifyCredentialAsync("ABCDEFG234", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CredentialVerificationDto
                {
                    Valid = false,
                    MemberName = "A Member",
                    Status = "Revoked"
                });
            var controller = new CredentialVerificationController(service.Object);

            var result = await controller.Verify("ABCDEFG234", CancellationToken.None);

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(((CredentialVerificationDto)ok!.Value!).Valid, Is.False);
        }

        [Test]
        public async Task Verify_ReturnsNotFoundForUnknownOrMalformedCode()
        {
            var service = new Mock<IIDCardService>();
            var controller = new CredentialVerificationController(service.Object);

            Assert.That(await controller.Verify("short", CancellationToken.None), Is.TypeOf<NotFoundResult>());
            service.Verify(x => x.VerifyCredentialAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
