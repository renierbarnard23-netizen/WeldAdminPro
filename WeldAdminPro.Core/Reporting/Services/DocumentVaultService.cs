using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class DocumentVaultService
    {
        private readonly string _vaultRoot =
            Path.Combine(
                Environment.CurrentDirectory,
                "DocumentVault");

        public DocumentVaultService()
        {
            if (!Directory.Exists(_vaultRoot))
            {
                Directory.CreateDirectory(_vaultRoot);
            }
        }

        public string SaveDocument(
            string sourceFile,
            string category)
        {
            if (!File.Exists(sourceFile))
            {
                return string.Empty;
            }

            var categoryFolder =
                Path.Combine(
                    _vaultRoot,
                    category);

            if (!Directory.Exists(categoryFolder))
            {
                Directory.CreateDirectory(
                    categoryFolder);
            }

            var fileName =
                Path.GetFileName(sourceFile);

            var destination =
                Path.Combine(
                    categoryFolder,
                    fileName);

            File.Copy(
                sourceFile,
                destination,
                true);

            return destination;
        }
    }
}
