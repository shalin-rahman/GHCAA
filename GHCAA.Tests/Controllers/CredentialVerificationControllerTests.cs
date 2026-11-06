using System.Security.Claims;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class CredentialVerificationControllerTests
    {
        private Mock<IIDCardService> _service = null!;
        private Mock<IActivityService> _activity = null!;
        private CredentialVerificationController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IIDCardService>();
            _activity = new Mock<IActivityService>();
            _controller = new CredentialVerificationController(_service.Object, _activity.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "7") }, "Test"))
                    }
                }
            };
        }

        private static IssuedCredentialDto Credential(string code, bool revoked = false) => new()
        {
            MemberId = 42,
            ShortCode = code,
            CredentialType = CredentialType.IdCard,
            IssuedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsRevoked = revoked
        };

        [Test]
        public async Task Verify_ReturnsOkForRevokedCredential()
        {
            _service.Setup(x => x.VerifyCredentialAsync("ABCDEFG234", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CredentialVerificationDto
                {
                    Valid = false,
                    MemberName = "A Member",
                    Status = "Revoked"
                });

            var result = await _controller.Verify("ABCDEFG234", CancellationToken.None);

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(((CredentialVerificationDto)ok!.Value!).Valid, Is.False);
        }

        [Test]
        public async Task Verify_ReturnsNotFoundForUnknownOrMalformedCode()
        {
            Assert.That(await _controller.Verify("short", CancellationToken.None), Is.TypeOf<NotFoundResult>());
            _service.Verify(x => x.VerifyCredentialAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Revoke_MissingReason_Returns400Problem()
        {
            var result = await _controller.Revoke("ABCDEFG234", new CredentialActionRequest { Reason = "  " }, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("A revocation reason is required."));
            _service.Verify(x => x.RevokeCredentialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-43")]
        public async Task Revoke_Success_WritesAuditEntryWithActor()
        {
            _service.Setup(x => x.RevokeCredentialAsync("ABCDEFG234", "Lost card", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Credential("ABCDEFG234", revoked: true));

            var result = await _controller.Revoke("ABCDEFG234", new CredentialActionRequest { Reason = " Lost card " }, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NoContentResult>());
            _activity.Verify(x => x.LogActivityAsync(42, Constants.Credentials.RevokedAuditType,
                It.Is<string>(d => d.Contains("ABCDEFG234") && d.Contains("Lost card")), 7,
                null, null, Constants.ActivitySources.Web, null, It.IsAny<CancellationToken>(), null), Times.Once);
        }

        [Test]
        public async Task Revoke_UnknownCode_ReturnsNotFoundWithoutAudit()
        {
            var result = await _controller.Revoke("ZZZZZZZZZZ", new CredentialActionRequest { Reason = "Lost card" }, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
            _activity.Verify(x => x.LogActivityAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()), Times.Never);
        }

        [Test]
        [Category("FR-43")]
        public async Task Reissue_Success_ReturnsNewCredentialAndWritesAuditEntry()
        {
            _service.Setup(x => x.ReissueCredentialAsync("ABCDEFG234", "Name changed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Credential("HJKLMNP234"));

            var result = await _controller.Reissue("ABCDEFG234", new CredentialActionRequest { Reason = "Name changed" }, CancellationToken.None);

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(((IssuedCredentialDto)ok!.Value!).ShortCode, Is.EqualTo("HJKLMNP234"));
            _activity.Verify(x => x.LogActivityAsync(42, Constants.Credentials.ReissuedAuditType,
                It.Is<string>(d => d.Contains("ABCDEFG234") && d.Contains("HJKLMNP234") && d.Contains("Name changed")), 7,
                null, null, Constants.ActivitySources.Web, null, It.IsAny<CancellationToken>(), null), Times.Once);
        }

        [Test]
        public async Task Reissue_UnknownCode_ReturnsNotFound()
        {
            var result = await _controller.Reissue("ZZZZZZZZZZ", new CredentialActionRequest { Reason = "Name changed" }, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Reissue_NewerValidCredentialExists_Returns409AndWritesNoAudit()
        {
            _service.Setup(x => x.ReissueCredentialAsync("ABCDEFG234", "Name changed", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new GHCAA.Domain.CredentialConflictException("newer one exists"));

            var result = await _controller.Reissue("ABCDEFG234", new CredentialActionRequest { Reason = "Name changed" }, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(409));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("newer one exists"));
            _activity.Verify(x => x.LogActivityAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()), Times.Never);
        }

        // Any other InvalidOperationException is a fault, not a conflict, and must not show its message as a 409.
        [Test]
        public void Reissue_UnrelatedInvalidOperation_IsNotTurnedInto409()
        {
            _service.Setup(x => x.ReissueCredentialAsync("ABCDEFG234", "Name changed", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("internal detail"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.Reissue("ABCDEFG234", new CredentialActionRequest { Reason = "Name changed" }, CancellationToken.None));
        }

        [Test]
        public async Task Reissue_MissingReason_Returns400Problem()
        {
            var result = await _controller.Reissue("ABCDEFG234", new CredentialActionRequest { Reason = "" }, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("A reason is required to reissue."));
            _service.Verify(x => x.ReissueCredentialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
