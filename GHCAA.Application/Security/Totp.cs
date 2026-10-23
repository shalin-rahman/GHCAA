using GHCAA.Domain;
using System.Security.Cryptography;
using System.Text;

namespace GHCAA.Application.Security
{
    // 95.6. RFC 6238 time-based codes: HMAC-SHA1 over the 30-second step count, cut to 6 digits.
    // This is the scheme every authenticator app reads from an otpauth:// QR code.
    public static class Totp
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static long StepAt(DateTimeOffset time) => time.ToUnixTimeSeconds() / Constants.Authenticator.StepSeconds;

        public static string Code(byte[] secret, long step)
        {
            Span<byte> counter = stackalloc byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(counter, step);
            Span<byte> hash = stackalloc byte[20];
            HMACSHA1.HashData(secret, counter, hash);

            // RFC 4226 section 5.3, dynamic truncation.
            var offset = hash[^1] & 0x0f;
            var value = ((hash[offset] & 0x7f) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
            var modulo = (int)Math.Pow(10, Constants.Authenticator.Digits);
            return (value % modulo).ToString(new string('0', Constants.Authenticator.Digits));
        }

        // Returns the step the code belongs to, or null. A step at or below lastUsedStep is
        // refused, so a code that was already accepted cannot be replayed inside its window.
        public static long? Match(byte[] secret, string code, DateTimeOffset now, long lastUsedStep)
        {
            if (code.Length != Constants.Authenticator.Digits || !code.All(char.IsAsciiDigit))
                return null;

            var expected = Encoding.ASCII.GetBytes(code);
            var current = StepAt(now);
            for (var step = current - Constants.Authenticator.DriftSteps; step <= current + Constants.Authenticator.DriftSteps; step++)
            {
                if (step <= lastUsedStep)
                    continue;
                if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Code(secret, step)), expected))
                    return step;
            }
            return null;
        }

        // RFC 4648 base32 with no padding, the form otpauth:// URIs carry.
        public static string ToBase32(byte[] data)
        {
            var text = new StringBuilder((data.Length * 8 + 4) / 5);
            int buffer = 0, bits = 0;
            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bits += 8;
                while (bits >= 5)
                {
                    text.Append(Base32Alphabet[(buffer >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }
            if (bits > 0)
                text.Append(Base32Alphabet[(buffer << (5 - bits)) & 31]);
            return text.ToString();
        }

        public static string OtpAuthUri(string issuer, string account, byte[] secret)
        {
            var label = Uri.EscapeDataString(issuer) + ":" + Uri.EscapeDataString(account);
            return $"otpauth://totp/{label}?secret={ToBase32(secret)}&issuer={Uri.EscapeDataString(issuer)}"
                + $"&algorithm=SHA1&digits={Constants.Authenticator.Digits}&period={Constants.Authenticator.StepSeconds}";
        }
    }
}
