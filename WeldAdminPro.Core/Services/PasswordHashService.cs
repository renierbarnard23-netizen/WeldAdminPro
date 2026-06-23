using System.Security.Cryptography;
using System.Text;

namespace WeldAdminPro.Core.Services
{
    public class PasswordHashService
    {
        public string Hash(
            string password)
        {
            using var sha =
                SHA256.Create();

            var bytes =
                Encoding.UTF8.GetBytes(password);

            var hash =
                sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}
