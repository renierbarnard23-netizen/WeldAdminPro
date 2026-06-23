using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WeldAdminPro.Core.Security
{
    public static class AuthService
    {
        public static string HashPassword(string password, int iterations = 100_000)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32);

            var saltB = Convert.ToBase64String(salt);
            var hashB = Convert.ToBase64String(hash);

            return $"pbkdf2_sha256${iterations}${saltB}${hashB}";
        }

        public static bool VerifyPassword(string stored, string candidate)
        {
            if (string.IsNullOrEmpty(stored) || !stored.StartsWith("pbkdf2_sha256"))
                return false;

            if (candidate is null) return false;

            // Format: $
            if (stored.Contains("$"))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4) return false;
                if (!int.TryParse(parts[1], out int it)) return false;

                var salt = Convert.FromBase64String(parts[2]);
                var hash = Convert.FromBase64String(parts[3]);

                return Verify(candidate, salt, it, hash);
            }

            // Fallback format
            var m = Regex.Match(stored,
                @"^pbkdf2_sha256(?<iters>\d{4,7})(?<salt>[A-Za-z0-9+/=]{20,32})(?<hash>[A-Za-z0-9+/=]{40,64})$");

            if (!m.Success) return false;

            if (!int.TryParse(m.Groups["iters"].Value, out int iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(m.Groups["salt"].Value);
                var hash = Convert.FromBase64String(m.Groups["hash"].Value);

                return Verify(candidate, salt, iterations, hash);
            }
            catch
            {
                return false;
            }
        }

        private static bool Verify(string candidate, byte[] salt, int iterations, byte[] expectedHash)
        {
            var candidateHash = Rfc2898DeriveBytes.Pbkdf2(
                candidate,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(candidateHash, expectedHash);
        }
    }
}