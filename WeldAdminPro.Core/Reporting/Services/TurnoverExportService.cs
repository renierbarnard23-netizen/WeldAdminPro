using System;
using System.IO;
using System.Linq;
using System.Text;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class TurnoverExportService
    {
        private readonly PdfReportService
            _pdfService;
        public string Export(
        TurnoverPackage package,
        string exportRoot)
        {
            var folderName =
            $"{package.ProjectNumber}_" +
            $"{package.ProjectName}";

        var exportPath =
            Path.Combine(
                exportRoot,
                folderName);

            Directory.CreateDirectory(
                exportPath);

            var documentsFolder =
                Path.Combine(
                    exportPath,
                    "Documents");

            Directory.CreateDirectory(
                documentsFolder);

            foreach (var document
                in package.Documents)
            {
                if (!File.Exists(
                        document.FilePath))
                {
                    continue;
                }

                var destination =
                    Path.Combine(
                        documentsFolder,
                        document.OriginalFileName);

                File.Copy(
                    document.FilePath,
                    destination,
                    true);
            }

            GenerateIndex(
                package,
                exportPath);

            GenerateWeldSummary(
                package,
                exportPath);

            GenerateRepairSummary(
                package,
                exportPath);

            _pdfService.GenerateTurnoverSummary(
                package,
                exportPath);

            _pdfService.GenerateWeldRegister(
                package,
                exportPath);

            _pdfService.GenerateNdtRegister(
                package,
                exportPath);

            return exportPath;
        }

        public TurnoverExportService()
        {
            _pdfService =
            new PdfReportService();
        }


        private void GenerateIndex(
            TurnoverPackage package,
            string exportPath)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "TURNOVER PACKAGE INDEX");

            builder.AppendLine();

            builder.AppendLine(
                $"Project: {package.ProjectName}");

            builder.AppendLine(
                $"Project Number: {package.ProjectNumber}");

            builder.AppendLine();

            builder.AppendLine(
                "DOCUMENTS");

            foreach (var document
                in package.Documents)
            {
                builder.AppendLine(
                    $"{document.DocumentNumber} | " +
                    $"{document.Title} | " +
                    $"Rev {document.Revision}");
            }

            File.WriteAllText(
                Path.Combine(
                    exportPath,
                    "MDR_Index.txt"),
                builder.ToString());
        }

        private void GenerateWeldSummary(
            TurnoverPackage package,
            string exportPath)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "WELD SUMMARY");

            builder.AppendLine();

            foreach (var weld
                in package.Welds)
            {
                builder.AppendLine(
                    $"{weld.WeldNumber} | " +
                    $"{weld.WorkflowStatus}");
            }

            File.WriteAllText(
                Path.Combine(
                    exportPath,
                    "WeldSummary.txt"),
                builder.ToString());
        }

        private void GenerateRepairSummary(
            TurnoverPackage package,
            string exportPath)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "REPAIR SUMMARY");

            builder.AppendLine();

            foreach (var repair
                in package.Repairs)
            {
                builder.AppendLine(
                    $"Repair #{repair.RepairNumber} | " +
                    $"{repair.Status}");
            }

            File.WriteAllText(
                Path.Combine(
                    exportPath,
                    "RepairSummary.txt"),
                builder.ToString());
        }
    }
}
