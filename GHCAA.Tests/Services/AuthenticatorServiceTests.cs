using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GHCAA.Tests.Services;

// 95.6: authenticator app step-up. The vectors come from RFC 6238 appendix B (SHA1 rows).
[TestFixture]
[Category("FR-39")]
public class TotpTests
{
    private static readonly byte[] RfcSecret = Encoding.ASCII.GetBytes("12345678901234567890");

    [TestCase(59L, "287082")]
    [TestCase(1111111109L, "081804")]
    [TestCase(1111111111L, "050471")]
    [TestCase(1234567890L, "005924")]
    [TestCase(2000000000L, "279037")]
    [TestCase(20000000000L, "353130")]
    public void Code_MatchesRfc6238Vectors(long unixSeconds, string expected)
    {
        var step = Totp.StepAt(DateTimeOffset.FromUnixTimeSeconds(unixSeconds));
        Totp.Code(RfcSecret, step).Should().Be(expected);
    }

    [Test]
    public void ToBase32_MatchesRfc4648Vector()
    {
        Totp.ToBase32(Encoding.ASCII.GetBytes("foobar")).Should().Be("MZXW6YTBOI");
    }

    [Test]
    public void Match_AcceptsOneStepEitherSide_AndRefusesTwo()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var step = Totp.StepAt(now);

        Totp.Match(RfcSecret, Totp.Code(RfcSecret, step - 1), now, 0).Should().Be(step - 1);
        Totp.Match(RfcSecret, Totp.Code(RfcSecret, step + 1), now, 0).Should().Be(step + 1);
        Totp.Match(RfcSecret, Totp.Code(RfcSecret, step + 2), now, 0).Should().BeNull();
    }

    [Test]
    public void Match_RefusesStepAlreadyUsed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var step = Totp.StepAt(now);

        Totp.Match(RfcSecret, Totp.Code(RfcSecret, step), now, step).Should().BeNull();
    }

    [TestCase("")]
    [TestCase("12345")]
    [TestCase("12345a")]
    public void Match_RefusesMalformedCode(string code)
    {
        Totp.Match(RfcSecret, code, DateTimeOffset.UtcNow, 0).Should().BeNull();
    }

    [Test]
    public void OtpAuthUri_CarriesIssuerAccountAndSecret()
    {
        var uri = Totp.OtpAuthUri("GHC AA", "admin@example.com", Encoding.ASCII.GetBytes("foobar"));

        uri.Should().StartWith("otpauth://totp/GHC%20AA:admin%40example.com?secret=MZXW6YTBOI&issuer=GHC%20AA");
        uri.Should().Contain("digits=6").And.Contain("period=30");
    }
}

[TestFixture]
[Category("FR-39")]
public class AuthenticatorServiceTests : TestBase
{
    private AuthenticatorService _service = null!;
    private int _userId;

    [SetUp]
    public async Task SetUp()
    {
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(x => x.GetConfigAsync())
            .ReturnsAsync(new OrgConfigDto { Branding = new BrandingDto { AppName = "Alumni App", ShortName = "GHCAA" } });
        _service = new AuthenticatorService(_context, new EphemeralDataProtectionProvider(), orgConfig.Object, NewActivity(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthenticatorService>.Instance);

        var member = await CreateAndSaveTestMemberAsync();
        _userId = (await CreateAndSaveTestUserAsync(member.Id, "admin1")).Id;
    }

    private static byte[] FromBase32(string text)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bytes = new List<byte>();
        int buffer = 0, bits = 0;
        foreach (var c in text)
        {
            buffer = (buffer << 5) | alphabet.IndexOf(c);
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)(buffer >> (bits - 8)));
                bits -= 8;
            }
        }
        return bytes.ToArray();
    }

    private static string CodeAt(byte[] secret, int stepOffset) =>
        Totp.Code(secret, Totp.StepAt(DateTimeOffset.UtcNow) + stepOffset);

    // Confirms with the current step, so later checks have to use the next step.
    private async Task<(byte[] Secret, IReadOnlyList<string> Codes)> EnrolAsync()
    {
        var setup = await _service.BeginSetupAsync(_userId, "admin@example.com");
        var secret = FromBase32(setup!.Secret);
        var codes = await _service.ConfirmAsync(_userId, CodeAt(secret, 0));
        DetachAll();
        return (secret, codes!.Codes);
    }

    [Test]
    public async Task BeginSetup_ReturnsQrAndUsesShortNameAsIssuer()
    {
        var setup = await _service.BeginSetupAsync(_userId, "admin@example.com");

        setup.Should().NotBeNull();
        setup!.QrCodeDataUri.Should().StartWith("data:image/png;base64,");
        setup.OtpAuthUri.Should().Contain("issuer=GHCAA");
        (await _service.GetStatusAsync(_userId)).Should().Be(new AuthenticatorStatusDto(false, true, 0));
    }

    [Test]
    public async Task BeginSetup_StoresSecretEncrypted()
    {
        var setup = await _service.BeginSetupAsync(_userId, "admin@example.com");

        var row = await _context.UserAuthenticators.AsNoTracking().SingleAsync();
        row.ProtectedSecret.Should().NotContain(setup!.Secret);
        Convert.FromBase64String(row.ProtectedSecret).Should().NotEqual(FromBase32(setup.Secret));
    }

    [Test]
    public async Task BeginSetup_RefusedOnceConfirmed()
    {
        await EnrolAsync();

        (await _service.BeginSetupAsync(_userId, "admin@example.com")).Should().BeNull();
    }

    [Test]
    public async Task Confirm_WrongCode_LeavesSetupPending()
    {
        await _service.BeginSetupAsync(_userId, "admin@example.com");

        (await _service.ConfirmAsync(_userId, "000000")).Should().BeNull();
        (await _service.GetStatusAsync(_userId)).Pending.Should().BeTrue();
    }

    [Test]
    public async Task Confirm_ReturnsTenRecoveryCodes()
    {
        var (_, codes) = await EnrolAsync();

        codes.Should().HaveCount(10).And.OnlyHaveUniqueItems();
        codes.Should().AllSatisfy(c => c.Should().MatchRegex("^[A-Z2-9]{5}-[A-Z2-9]{5}$"));
        (await _service.GetStatusAsync(_userId)).Should().Be(new AuthenticatorStatusDto(true, false, 10));
    }

    [Test]
    public async Task Verify_AcceptsCodeOnce()
    {
        var (secret, _) = await EnrolAsync();
        var code = CodeAt(secret, 1);

        (await _service.VerifyAsync(_userId, code)).Should().BeTrue();
        (await _service.VerifyAsync(_userId, code)).Should().BeFalse();
    }

    [Test]
    public async Task Verify_RefusesCodeUsedToConfirm()
    {
        var (secret, _) = await EnrolAsync();

        (await _service.VerifyAsync(_userId, CodeAt(secret, 0))).Should().BeFalse();
    }

    [Test]
    public async Task Verify_RefusesPendingSetup()
    {
        var setup = await _service.BeginSetupAsync(_userId, "admin@example.com");

        (await _service.VerifyAsync(_userId, CodeAt(FromBase32(setup!.Secret), 0))).Should().BeFalse();
    }

    [Test]
    public async Task Verify_RecoveryCodeWorksOnce_IgnoringCaseAndDash()
    {
        var (_, codes) = await EnrolAsync();
        var typed = codes[0].Replace("-", "").ToLowerInvariant();

        (await _service.VerifyAsync(_userId, typed)).Should().BeTrue();
        (await _service.VerifyAsync(_userId, codes[0])).Should().BeFalse();
        (await _service.GetStatusAsync(_userId)).RecoveryCodesLeft.Should().Be(9);
    }

    [Test]
    public async Task Verify_LocksAfterMaxWrongCodes_EvenForTheRightCode()
    {
        var (secret, codes) = await EnrolAsync();

        for (var i = 0; i < Constants.Authenticator.MaxFailedAttempts; i++)
            (await _service.VerifyAsync(_userId, "000000")).Should().BeFalse();

        (await _service.VerifyAsync(_userId, CodeAt(secret, 1))).Should().BeFalse();
        (await _service.VerifyAsync(_userId, codes[0])).Should().BeFalse();
        var row = await _context.UserAuthenticators.AsNoTracking().SingleAsync();
        row.LockedUntil.Should().BeAfter(DateTime.UtcNow);
        row.FailedAttempts.Should().Be(0);
    }

    [Test]
    public async Task Verify_GoodCodeClearsFailureCount()
    {
        var (secret, _) = await EnrolAsync();

        for (var i = 0; i < Constants.Authenticator.MaxFailedAttempts - 1; i++)
            await _service.VerifyAsync(_userId, "000000");
        (await _service.VerifyAsync(_userId, CodeAt(secret, 1))).Should().BeTrue();

        var row = await _context.UserAuthenticators.AsNoTracking().SingleAsync();
        row.FailedAttempts.Should().Be(0);
        row.LockedUntil.Should().BeNull();
    }

    [Test]
    public async Task Confirm_SecondConfirmGetsNoCodes()
    {
        var setup = await _service.BeginSetupAsync(_userId, "admin@example.com");
        var secret = FromBase32(setup!.Secret);
        DetachAll();

        (await _service.ConfirmAsync(_userId, CodeAt(secret, 0))).Should().NotBeNull();
        (await _service.ConfirmAsync(_userId, CodeAt(secret, 1))).Should().BeNull();
    }

    [Test]
    public async Task Verify_FalseWhenSecretCannotBeDecrypted()
    {
        var (secret, _) = await EnrolAsync();
        await _context.UserAuthenticators.ExecuteUpdateAsync(s => s.SetProperty(a => a.ProtectedSecret, "bm90LWEta2V5"));

        (await _service.VerifyAsync(_userId, CodeAt(secret, 1))).Should().BeFalse();
    }

    [Test]
    public async Task RegenerateRecoveryCodes_ReplacesOldCodes()
    {
        var (_, oldCodes) = await EnrolAsync();

        var fresh = await _service.RegenerateRecoveryCodesAsync(_userId);
        DetachAll();

        fresh!.Codes.Should().HaveCount(10);
        (await _service.VerifyAsync(_userId, oldCodes[0])).Should().BeFalse();
        (await _service.VerifyAsync(_userId, fresh.Codes[0])).Should().BeTrue();
    }

    [Test]
    public async Task RegenerateRecoveryCodes_NullWhenNotEnrolled()
    {
        (await _service.RegenerateRecoveryCodesAsync(_userId)).Should().BeNull();
    }

    [Test]
    public async Task Remove_DeletesRow_AndReportsWhenNothingToRemove()
    {
        await EnrolAsync();

        (await _service.RemoveAsync(_userId)).Should().BeTrue();
        (await _service.RemoveAsync(_userId)).Should().BeFalse();
        (await _service.GetStatusAsync(_userId)).Should().Be(new AuthenticatorStatusDto(false, false, 0));
    }

    [Test]
    public async Task ResetByAdmin_RemovesApp_SignsUserOut_AndWritesAuditRow()
    {
        await EnrolAsync();
        var stampBefore = (await _context.Users.AsNoTracking().FirstAsync(u => u.Id == _userId)).SecurityStamp;

        (await _service.ResetByAdminAsync(_userId, adminId: 99)).Should().BeTrue();
        DetachAll();

        (await _service.GetStatusAsync(_userId)).Enrolled.Should().BeFalse();
        (await _context.Users.AsNoTracking().FirstAsync(u => u.Id == _userId)).SecurityStamp.Should().NotBe(stampBefore);
        var audit = await _context.ActivityLogs.AsNoTracking().SingleAsync(a => a.ActivityType == Constants.Authenticator.AdminResetAuditType);
        audit.ActorId.Should().Be(99);
    }

    [Test]
    public async Task ResetByAdmin_FalseAndNoSideEffects_WhenNoApp()
    {
        var stampBefore = (await _context.Users.AsNoTracking().FirstAsync(u => u.Id == _userId)).SecurityStamp;

        (await _service.ResetByAdminAsync(_userId, adminId: 99)).Should().BeFalse();

        (await _context.Users.AsNoTracking().FirstAsync(u => u.Id == _userId)).SecurityStamp.Should().Be(stampBefore);
        (await _context.ActivityLogs.AnyAsync(a => a.ActivityType == Constants.Authenticator.AdminResetAuditType)).Should().BeFalse();
    }

    [Test]
    public async Task PasswordMatches_OnlyForTheRightPassword()
    {
        (await _service.PasswordMatchesAsync(_userId, "TestPassword123")).Should().BeTrue();
        (await _service.PasswordMatchesAsync(_userId, "wrong")).Should().BeFalse();
        (await _service.PasswordMatchesAsync(-1, "TestPassword123")).Should().BeFalse();
    }

    [Test]
    public async Task RecoveryCode_TypedLowerCaseWithoutDash_StillMatches()
    {
        var (_, codes) = await EnrolAsync();

        (await _service.VerifyAsync(_userId, codes[0].Replace("-", "").ToLowerInvariant())).Should().BeTrue();
    }
}

[TestFixture]
[Category("FR-39")]
public class StepUpVerifyDtoTests
{
    private static bool IsValid(StepUpVerifyDto dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), validateAllProperties: true);

    [TestCase("123456", null, true)]
    [TestCase("123456", "email", true)]
    [TestCase("123456", "app", true)]
    [TestCase("ABCDE-FGH23", "app", true)]
    [TestCase("abcdefgh23", "app", true)]
    [TestCase("ABCDE-FGH23", null, false)]
    [TestCase("ABCDE-FGH23", "email", false)]
    [TestCase("12345", "app", false)]
    [TestCase("123456", "sms", false)]
    public void Validates_CodeAgainstMethod(string code, string? method, bool expected)
    {
        IsValid(new StepUpVerifyDto { Code = code, Method = method }).Should().Be(expected);
    }
}
