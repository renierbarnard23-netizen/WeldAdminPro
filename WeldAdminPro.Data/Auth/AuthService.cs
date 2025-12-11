using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WeldAdminPro.Data.Auth
{
    public static class AuthService
    {
        public static bool VerifyPassword(string stored, string candidate)
        {
            if (string.IsNullOrEmpty(stored) || !stored.StartsWith("pbkdf2_sha256"))
                return false;

            if (stored.Contains("$"))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4) return false;
                if (!int.TryParse(parts[1], out int iterations)) return false;

                var salt = Convert.FromBase64String(parts[2]);
                var hash = Convert.FromBase64String(parts[3]);

                return Verify(candidate, salt, iterations, hash);
            }

            var m = Regex.Match(
                stored,
                @"^pbkdf2_sha256(?<iters>\d{4,7})(?<salt>[A-Za-z0-9+/=]{20,32})(?<hash>[A-Za-z0-9+/=]{40,64})$"
            );
            if (!m.Success) return false;

            int iters = int.Parse(m.Groups["iters"].Value);
            var saltB = Convert.FromBase64String(m.Groups["salt"].Value);
            var hashB = Convert.FromBase64String(m.Groups["hash"].Value);

            return Verify(candidate, saltB, iters, hashB);
        }

        private static bool Verify(string candidate, byte[] salt, int iterations, byte[] expectedHash)
        {
            using var derive = new Rfc2898DeriveBytes(
                candidate, 
                salt, 
                iterations, 
                HashAlgorithmName.SHA256
            );
            var testHash = derive.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(testHash, expectedHash);
        }
    }
}
