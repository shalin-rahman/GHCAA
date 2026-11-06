using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// 84.60: the expiry date a new credential gets from Workflow.CredentialValidityMonths, and what
// verification says once that date has passed.
[TestFixture]
public class IDCardServiceCredentialTests : TestBase
{
    private Mock<IOrgConfigService> _orgConfig = null!;
    private IDCardService _service = null!;

    [SetUp]
    public void CredentialSetup()
    {
        _orgConfig = new Mock<IOrgConfigService>();
        UseValidityMonths(12);
        _service = new IDCardService(_context, _orgConfig.Object, new Mock<IFileStorageService>().Object,
            new Mock<IWebHostEnvironment>().Object);
    }

    private void UseValidityMonths(int months) =>
        _orgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto
        {
            Contact = new ContactDto { PortalBaseUrl = "https://portal.test" },
            Workflow = new WorkflowDto { CredentialValidityMonths = months }
        });

    private async Task<IssuedCredential> IssueCertificateAsync()
    {
        var member = await CreateAndSaveTestMemberAsync();
        await _service.GenerateCertificateDataUriAsync(member.Id);
        return await _context.IssuedCredentials.AsNoTracking().SingleAsync(c => c.MemberId == member.Id);
    }

    [Category("FR-06")]
    [Test]
    public async Task Issue_SetsExpiryFromTheValiditySetting()
    {
        UseValidityMonths(6);

        var credential = await IssueCertificateAsync();

        credential.ExpiresOn.Should().Be(credential.IssuedOn.AddMonths(6));
    }

    [Category("FR-06")]
    [Test]
    public async Task Issue_WithZeroMonths_NeverExpires()
    {
        UseValidityMonths(0);

        var credential = await IssueCertificateAsync();

        credential.ExpiresOn.Should().BeNull();
        (await _service.VerifyCredentialAsync(credential.ShortCode))!.Valid.Should().BeTrue();
    }

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(Constants.Credentials.MaxValidityMonths, true)]
    [TestCase(Constants.Credentials.MaxValidityMonths + 1, false)]
    public void ValiditySetting_RangeIsChecked(int months, bool expectedValid)
    {
        var dto = new WorkflowDto { CredentialValidityMonths = months };
        var errors = new List<ValidationResult>();

        Validator.TryValidateObject(dto, new ValidationContext(dto), errors, validateAllProperties: true)
            .Should().Be(expectedValid);
    }

    [Category("FR-06")]
    [Test]
    public async Task Verify_ExpiredCredential_IsNotValid()
    {
        var credential = await IssueCertificateAsync();
        var stored = await _context.IssuedCredentials.SingleAsync(c => c.Id == credential.Id);
        stored.ExpiresOn = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync();

        var result = await _service.VerifyCredentialAsync(credential.ShortCode);

        result!.Valid.Should().BeFalse();
        result.Status.Should().Be("Expired");
    }

    // 84.68 below: the admin credential list, revoke and reissue.
    [Test]
    public async Task GetMemberCredentials_UnknownMember_ReturnsNull()
    {
        (await _service.GetMemberCredentialsAsync(999_999)).Should().BeNull();
    }

    [Test]
    public async Task GetMemberCredentials_ListsNewestFirst()
    {
        var first = await IssueCertificateAsync();
        await _service.GenerateIDCardDataUriAsync(first.MemberId);

        var list = await _service.GetMemberCredentialsAsync(first.MemberId);

        list.Should().HaveCount(2);
        list![1].ShortCode.Should().Be(first.ShortCode);
        list[0].CredentialType.Should().Be(CredentialType.IdCard);
    }

    [Test]
    public async Task Revoke_UnknownCode_ReturnsNull()
    {
        (await _service.RevokeCredentialAsync("ZZZZZZZZZZ", "Lost")).Should().BeNull();
    }

    [Category("FR-06")]
    [Test]
    public async Task Revoke_MarksCredentialAndVerificationFails()
    {
        var credential = await IssueCertificateAsync();

        var revoked = await _service.RevokeCredentialAsync(credential.ShortCode, "Lost");

        revoked!.IsRevoked.Should().BeTrue();
        revoked.RevokedReason.Should().Be("Lost");
        (await _service.VerifyCredentialAsync(credential.ShortCode))!.Valid.Should().BeFalse();
    }

    [Category("FR-06")]
    [Test]
    public async Task Reissue_RevokesOldCodeAndIssuesNewOneWithCurrentExpiry()
    {
        var old = await IssueCertificateAsync();
        UseValidityMonths(3);

        var replacement = await _service.ReissueCredentialAsync(old.ShortCode, "Name changed");

        replacement!.ShortCode.Should().NotBe(old.ShortCode);
        replacement.CredentialType.Should().Be(CredentialType.MembershipCertificate);
        replacement.MemberId.Should().Be(old.MemberId);
        replacement.ExpiresOn.Should().Be(replacement.IssuedOn.AddMonths(3));
        (await _service.VerifyCredentialAsync(old.ShortCode))!.Valid.Should().BeFalse();
        (await _service.VerifyCredentialAsync(replacement.ShortCode))!.Valid.Should().BeTrue();
    }

    [Test]
    public async Task Reissue_AlreadyRevoked_KeepsFirstReason()
    {
        var old = await IssueCertificateAsync();
        await _service.RevokeCredentialAsync(old.ShortCode, "Lost");

        await _service.ReissueCredentialAsync(old.ShortCode, "Found again");

        var stored = await _context.IssuedCredentials.AsNoTracking().SingleAsync(c => c.Id == old.Id);
        stored.RevokedReason.Should().Be("Lost");
    }

    [Category("FR-06")]
    [Test]
    public async Task Reissue_OldCodeWhileNewerOneIsValid_IsRefusedAndIssuesNothing()
    {
        var old = await IssueCertificateAsync();
        await _service.ReissueCredentialAsync(old.ShortCode, "Name changed");
        var countBefore = await _context.IssuedCredentials.CountAsync();

        var act = () => _service.ReissueCredentialAsync(old.ShortCode, "Again");

        await act.Should().ThrowAsync<CredentialConflictException>();
        (await _context.IssuedCredentials.CountAsync()).Should().Be(countBefore);
    }

    [Test]
    public async Task Reissue_OldCodeAfterNewerOneWasRevoked_IsAllowed()
    {
        var old = await IssueCertificateAsync();
        var newer = await _service.ReissueCredentialAsync(old.ShortCode, "Name changed");
        await _service.RevokeCredentialAsync(newer!.ShortCode, "Lost");

        (await _service.ReissueCredentialAsync(old.ShortCode, "Lost again")).Should().NotBeNull();
    }

    [Test]
    public async Task Reissue_UnknownCode_ReturnsNull()
    {
        (await _service.ReissueCredentialAsync("ZZZZZZZZZZ", "Lost")).Should().BeNull();
    }
}
