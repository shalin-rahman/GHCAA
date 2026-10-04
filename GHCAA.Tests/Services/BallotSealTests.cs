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
    private const int ElectionId = 7;
    private const int Current = Constants.Elections.BallotSealVersion;
    private const int Legacy = Constants.Elections.BallotSealLegacyVersion;

    [OneTimeTearDown]
    public void DisposeKey() => Key.Dispose();

    [Test]
    [Category("FR-39")]
    public void Open_ReturnsWhatWasSealed()
    {
        var sealedValue = BallotSeal.Seal(PublicKey, "[{\"S\":1,\"N\":[2]}]", ElectionId, Current);

        sealedValue.Should().StartWith(Constants.Elections.BallotSealPrefix);
        BallotSeal.Open(Key, sealedValue, ElectionId, Current).Should().Be("[{\"S\":1,\"N\":[2]}]");
    }

    [Test]
    [Category("FR-39")]
    public void Open_ReadsALegacyBallotInALegacyElection()
    {
        var sealedValue = BallotSeal.Seal(PublicKey, "choices", ElectionId, Legacy);

        sealedValue.Should().NotStartWith(Constants.Elections.BallotSealPrefix);
        BallotSeal.Open(Key, sealedValue, ElectionId, Legacy).Should().Be("choices");
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsForAnotherElection()
    {
        var sealedValue = BallotSeal.Seal(PublicKey, "choices", ElectionId, Current);

        var act = () => BallotSeal.Open(Key, sealedValue, ElectionId + 1, Current);

        act.Should().Throw<CryptographicException>();
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsWhenTheFormatDoesNotMatchTheElection()
    {
        var legacy = BallotSeal.Seal(PublicKey, "choices", ElectionId, Legacy);
        var current = BallotSeal.Seal(PublicKey, "choices", ElectionId, Current);

        // A legacy ballot carries no election id, so a version 2 election must not take it.
        ((Action)(() => BallotSeal.Open(Key, legacy, ElectionId, Current))).Should().Throw<CryptographicException>();
        ((Action)(() => BallotSeal.Open(Key, current, ElectionId, Legacy))).Should().Throw<CryptographicException>();
        // With the prefix stripped, the bytes were still sealed with the election id.
        var stripped = current[Constants.Elections.BallotSealPrefix.Length..];
        ((Action)(() => BallotSeal.Open(Key, stripped, ElectionId, Legacy))).Should().Throw<CryptographicException>();
    }

    [Test]
    [Category("FR-39")]
    public void Seal_RefusesAnUnknownVersion()
    {
        var act = () => BallotSeal.Seal(PublicKey, "choices", ElectionId, 99);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [Category("FR-39")]
    public void Seal_GivesADifferentValueEachTime()
    {
        BallotSeal.Seal(PublicKey, "same", ElectionId, Current).Should().NotBe(BallotSeal.Seal(PublicKey, "same", ElectionId, Current));
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsWhenTheValueWasChanged()
    {
        var bytes = Convert.FromBase64String(BallotSeal.Seal(PublicKey, "choices", ElectionId, Current)[Constants.Elections.BallotSealPrefix.Length..]);
        bytes[^1] ^= 1;

        var act = () => BallotSeal.Open(Key, Constants.Elections.BallotSealPrefix + Convert.ToBase64String(bytes), ElectionId, Current);

        act.Should().Throw<CryptographicException>();
    }

    [Test]
    [Category("FR-39")]
    public void Open_ThrowsForAnotherKey()
    {
        using var other = RSA.Create(Constants.Elections.BallotKeyMinBits);

        var act = () => BallotSeal.Open(other, BallotSeal.Seal(PublicKey, "choices", ElectionId, Current), ElectionId, Current);

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
