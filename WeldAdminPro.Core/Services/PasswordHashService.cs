using System;
using System.Security.Cryptography;

namespace WeldAdminPro.Core.Services
{
    public class PasswordHashService
    {
        private const int DefaultIterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string Hash(string password)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                DefaultIterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"pbkdf2_sha256${DefaultIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            if (password is null || string.IsNullOrWhiteSpace(storedHash))
                return false;

            if (storedHash.StartsWith("pbkdf2_sha256$", StringComparison.Ordinal))
            {
                return VerifyPbkdf2(password, storedHash);
            }

            return VerifyLegacySha256(password, storedHash);
        }

        public bool IsLegacyHash(string storedHash)
        {
            return !string.IsNullOrWhiteSpace(storedHash)
                && !storedHash.StartsWith(
                    "pbkdf2_sha256$",
                    StringComparison.Ordinal);
        }

        private static bool VerifyPbkdf2(
            string password,
            string storedHash)
        {
            try
            {
                var parts = storedHash.Split('$');

                if (parts.Length != 4)
                    return false;

                if (!int.TryParse(parts[1], out var iterations))
                    return false;

                if (iterations <= 0)
                    return false;

                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);

                if (salt.Length == 0 || expectedHash.Length == 0)
                    return false;

                var candidateHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(
                    candidateHash,
                    expectedHash);
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifyLegacySha256(
            string password,
            string storedHash)
        {
            try
            {
                using var sha = SHA256.Create();

                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var candidateHash = sha.ComputeHash(bytes);

                var expectedHash =
                    Convert.FromBase64String(storedHash);

                return CryptographicOperations.FixedTimeEquals(
                    candidateHash,
                    expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
