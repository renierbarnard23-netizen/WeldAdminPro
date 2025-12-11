// WeldAdminPro.Core/Security/AuthService.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WeldAdminPro.Core.Security
{
    public static class AuthService
    {
        // Hash a password (PBKDF2-SHA256) and return stored format:
        // pbkdf2_sha256$<iterations>$<saltb64>$<hashb64>
        public static string HashPassword(string password, int iterations = 100_000)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32); // 256-bit
            var saltB = Convert.ToBase64String(salt);
            var hashB = Convert.ToBase64String(hash);
            return $"pbkdf2_sha256${iterations}${saltB}${hashB}";
        }

        // Returns true if candidate password verifies against stored string.
        // Accepts two stored formats:
        // 1) pbkdf2_sha256$iterations$base64salt$base64hash
        // 2) pbkdf2_sha256{iterations}{base64salt}{base64hash}  (concatenated)
        public static bool VerifyPassword(string stored, string candidate)
        {
            if (string.IsNullOrEmpty(stored) || !stored.StartsWith("pbkdf2_sha256")) return false;
            if (candidate is null) return false;

            // Try format with separators first
            if (stored.Contains("$"))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4) return false;
                if (!int.TryParse(parts[1], out int it)) return false;
                var salt = Convert.FromBase64String(parts[2]);
                var hash = Convert.FromBase64String(parts[3]);
                return VerifyPbkdf2(candidate, salt, it, hash);
            }

            // Fallback: parse concatenated form (tolerant lengths)
            var m = Regex.Match(stored, @"^pbkdf2_sha256(?<iters>\d{4,7})(?<salt>[A-Za-z0-9+/=]{20,32})(?<hash>[A-Za-z0-9+/=]{40,64})$");
            if (!m.Success) return false;

            if (!int.TryParse(m.Groups["iters"].Value, out int iterations)) return false;
            var saltB = m.Groups["salt"].Value;
            var hashB = m.Groups["hash"].Value;

            try
            {
                var salt = Convert.FromBase64String(saltB);
                var hash = Convert.FromBase64String(hashB);
                return VerifyPbkdf2(candidate, salt, iterations, hash);
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifyPbkdf2(string candidate, byte[] salt, int iterations, byte[] expectedHash)
        {
            using var derive = new Rfc2898DeriveBytes(candidate, salt, iterations, HashAlgorithmName.SHA256);
            var candidateHash = derive.GetBytes(expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(candidateHash, expectedHash);
        }
    }
}
