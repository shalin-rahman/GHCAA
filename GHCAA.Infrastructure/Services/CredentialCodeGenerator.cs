using System.Security.Cryptography;

namespace GHCAA.Infrastructure.Services
{
    public static class CredentialCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Create()
        {
            Span<byte> bytes = stackalloc byte[10];
            RandomNumberGenerator.Fill(bytes);
            Span<char> code = stackalloc char[10];
            for (var i = 0; i < code.Length; i++)
                code[i] = Alphabet[bytes[i] % Alphabet.Length];
            return new string(code);
        }
    }
}
