using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class TurnoverPackageService
    {
        private readonly PdfExportService
            _pdfExportService = new();

        private readonly PdfMergeService
            _mergeService = new();

        public void Generate(
            WeldDataBook dataBook,
            string outputFolder)
        {
            Directory.CreateDirectory(
                outputFolder);

            var databookPath =
                Path.Combine(
                    outputFolder,
                    "DataBook.pdf");

            // =====================================
            // GENERATE MAIN DATABOOK
            // =====================================

            _pdfExportService.Export(
                dataBook,
                databookPath);

            // =====================================
            // BUILD MERGE LIST
            // =====================================

            var files =
                new List<string>
                {
                    databookPath
                };

            foreach (var attachment
                in dataBook.Attachments)
            {
                if (File.Exists(
                    attachment.FilePath))
                {
                    files.Add(
                        attachment.FilePath);
                }
            }

            // =====================================
            // FINAL PACKAGE
            // =====================================

            var finalPath =
                Path.Combine(
                    outputFolder,
                    "FinalTurnoverPackage.pdf");

            _mergeService.Merge(
                finalPath,
                files);
        }
    }
}
