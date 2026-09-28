using System;
using System.Security.Cryptography;
using System.Text;
using static GHCAA.Domain.Constants;

namespace GHCAA.Infrastructure.Services;

// Spec 023 FR-001. Seals a ballot's choices under the returning officer's RSA public key.
// Each ballot gets its own AES-256-GCM key, and that key is wrapped with RSA-OAEP SHA-256.
// The sealed value is base64 of: wrapped key, 12-byte nonce, 16-byte tag, ciphertext.
public static class BallotSeal
{
    private const int NonceBytes = 12;
    private const int TagBytes = 16;
    private const int AesKeyBytes = 32;
    private static readonly byte[] StandardExponent = [1, 0, 1];

    // Returns the fingerprint of a valid key, or null when the key is not RSA SPKI of the
    // allowed size with the usual exponent. The upper bound stops a huge key from slowing
    // every vote.
    public static string? Fingerprint(string publicKeySpkiBase64)
    {
        try
        {
            var spki = Convert.FromBase64String(publicKeySpkiBase64);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(spki, out var read);
            if (read != spki.Length || rsa.KeySize < Elections.BallotKeyMinBits || rsa.KeySize > Elections.BallotKeyMaxBits) return null;
            if (!rsa.ExportParameters(false).Exponent.AsSpan().SequenceEqual(StandardExponent)) return null;
            return Convert.ToHexStringLower(SHA256.HashData(spki));
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return null;
        }
    }

    // The fingerprint of the public half of a PKCS#8 private key, so the count can check it was
    // given the key the election was sealed under.
    public static string PrivateKeyFingerprint(RSA privateKey) =>
        Convert.ToHexStringLower(SHA256.HashData(privateKey.ExportSubjectPublicKeyInfo()));

    public static string Seal(string publicKeySpkiBase64, string plaintext)
    {
        var body = Encoding.UTF8.GetBytes(plaintext);
        // JSON ignores trailing spaces, so padding with them keeps the plaintext readable.
        var block = Elections.SealedChoicesBlockBytes;
        var padded = new byte[(body.Length / block + 1) * block];
        body.CopyTo(padded, 0);
        padded.AsSpan(body.Length).Fill((byte)' ');

        var key = RandomNumberGenerator.GetBytes(AesKeyBytes);
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeySpkiBase64), out _);
            var wrapped = rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);

            var nonce = RandomNumberGenerator.GetBytes(NonceBytes);
            var tag = new byte[TagBytes];
            var cipher = new byte[padded.Length];
            using (var aes = new AesGcm(key, TagBytes)) aes.Encrypt(nonce, padded, cipher, tag);

            var sealedBytes = new byte[wrapped.Length + NonceBytes + TagBytes + cipher.Length];
            wrapped.CopyTo(sealedBytes, 0);
            nonce.CopyTo(sealedBytes, wrapped.Length);
            tag.CopyTo(sealedBytes, wrapped.Length + NonceBytes);
            cipher.CopyTo(sealedBytes, wrapped.Length + NonceBytes + TagBytes);
            return Convert.ToBase64String(sealedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(body);
            CryptographicOperations.ZeroMemory(padded);
        }
    }

    // Throws CryptographicException when the value was not sealed under this key or was changed.
    public static string Open(RSA privateKey, string sealedValue)
    {
        var sealedBytes = Convert.FromBase64String(sealedValue);
        var wrappedLength = privateKey.KeySize / 8;
        if (sealedBytes.Length < wrappedLength + NonceBytes + TagBytes) throw new CryptographicException("The sealed ballot is too short.");

        var key = privateKey.Decrypt(sealedBytes.AsSpan(0, wrappedLength).ToArray(), RSAEncryptionPadding.OaepSHA256);
        var plain = new byte[sealedBytes.Length - wrappedLength - NonceBytes - TagBytes];
        try
        {
            using var aes = new AesGcm(key, TagBytes);
            aes.Decrypt(
                sealedBytes.AsSpan(wrappedLength, NonceBytes),
                sealedBytes.AsSpan(wrappedLength + NonceBytes + TagBytes),
                sealedBytes.AsSpan(wrappedLength + NonceBytes, TagBytes),
                plain);
            return Encoding.UTF8.GetString(plain).TrimEnd(' ');
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key);
            CryptographicOperations.ZeroMemory(plain);
        }
    }
}
