using System.Security.Cryptography;
using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
public sealed class BallotSealTests
{
    private static readonly RSA Key = RSA.Create(Constants.Elections.BallotKeyMinBits);
    private static readonly string PublicKey = Convert.ToBase64String(Key.ExportSubjectPublicKeyInfo());

    [OneTimeTearDown]
    public void DisposeKey() => Key.Dispose();

    [Test]
    [Category("FR-39")]
    public void Open_ReturnsWhatWasSealed()
    {
        var sealedValue = BallotSeal.Seal(PublicKey, "[{\"S\":1,\"N\":[2]}]");

        BallotSeal.Open(Key, sealedValue).Should().Be("[{\"S\":1,\"N\":[2]}]");
    }

    [Test]
    [Category("FR-39")]
    public void Seal_GivesADifferentValueEachTime()
    {
        BallotSeal.Seal(PublicKey, "same").Should().NotBe(BallotSeal.Seal(PublicKey, "same"));
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsWhenTheValueWasChanged()
    {
        var bytes = Convert.FromBase64String(BallotSeal.Seal(PublicKey, "choices"));
        bytes[^1] ^= 1;

        var act = () => BallotSeal.Open(Key, Convert.ToBase64String(bytes));

        act.Should().Throw<CryptographicException>();
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsForAnotherKey()
    {
        using var other = RSA.Create(Constants.Elections.BallotKeyMinBits);

        var act = () => BallotSeal.Open(other, BallotSeal.Seal(PublicKey, "choices"));

        act.Should().Throw<CryptographicException>();
    }

    [Test]
    [Category("FR-39")]
    public void Fingerprint_RefusesAShortKeyAndJunk()
    {
        using var weak = RSA.Create(2048);

        BallotSeal.Fingerprint(Convert.ToBase64String(weak.ExportSubjectPublicKeyInfo())).Should().BeNull();
        BallotSeal.Fingerprint("not a key").Should().BeNull();
        BallotSeal.Fingerprint(PublicKey).Should().Be(BallotSeal.PrivateKeyFingerprint(Key)).And.HaveLength(64);
    }

    [Test]
    [Category("FR-39")]
    public void Fingerprint_RefusesAnOversizedKeyAndAnOddExponent()
    {
        var modulus = Key.ExportParameters(false).Modulus!;

        BallotSeal.Fingerprint(PublicKeyOf(modulus, [3])).Should().BeNull();
        var huge = RandomNumberGenerator.GetBytes((Constants.Elections.BallotKeyMaxBits + 1024) / 8);
        huge[0] |= 0x80;
        huge[^1] |= 1;
        BallotSeal.Fingerprint(PublicKeyOf(huge, [1, 0, 1])).Should().BeNull();
    }

    private static string PublicKeyOf(byte[] modulus, byte[] exponent)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters { Modulus = modulus, Exponent = exponent });
        return Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
    }
}
