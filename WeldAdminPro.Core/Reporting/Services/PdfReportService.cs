using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class PdfReportService
    {
        public string GenerateTurnoverSummary(
            TurnoverPackage package,
            string exportFolder)
        {
            if (string.IsNullOrWhiteSpace(
        exportFolder))
            {
                throw new InvalidOperationException(
                    "Export folder is required.");
            }

            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package));
            }

            using var document =
            new PdfDocument();

            document.Info.Title =
                "Turnover Summary";

            var page =
                document.AddPage();

            var graphics =
                XGraphics.FromPdfPage(page);

            var titleFont =
                new XFont(
                    "Arial",
                    20,
                    XFontStyle.Bold);

            var bodyFont =
                new XFont(
                    "Arial",
                    12,
                    XFontStyle.Regular);

            double y = 40;

            graphics.DrawString(
                "TURNOVER SUMMARY",
                titleFont,
                XBrushes.Black,
                new XRect(
                    0,
                    y,
                    page.Width,
                    40),
                XStringFormats.TopCenter);

            y += 60;

            graphics.DrawString(
                $"Project: {package.ProjectName}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 25;

            graphics.DrawString(
                $"Project Number: {package.ProjectNumber}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 40;

            graphics.DrawString(
                $"Total Welds: {package.Welds?.Count ?? 0}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 25;

            graphics.DrawString(
                $"Total Repairs: {package.Repairs?.Count ?? 0}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 40;

            graphics.DrawString(
                "WARNINGS",
                titleFont,
                XBrushes.DarkRed,
                40,
                y);

            y += 35;

            if (!package.Warnings?.Any() ?? true)
            {
                graphics.DrawString(
                    "No warnings.",
                    bodyFont,
                    XBrushes.DarkGreen,
                    40,
                    y);
            }
            else
            {
                foreach (var warning
                    in package.Warnings
                    ?? Enumerable.Empty<string>())
                {
                    graphics.DrawString(
                        $"- {warning}",
                        bodyFont,
                        XBrushes.Red,
                        40,
                        y);

                    y += 25;
                }
            }

            Directory.CreateDirectory(exportFolder);

            var filePath =
                Path.Combine(
                    exportFolder,
                    "TurnoverSummary.pdf");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            document.Save(filePath);

            return filePath;
}


        public string GenerateNdtRegister(
                TurnoverPackage package,
                string exportFolder)
        {
            if (string.IsNullOrWhiteSpace(
        exportFolder))
            {
                throw new InvalidOperationException(
                    "Export folder is required.");
            }

            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package));
            }

            using var document =
                new PdfDocument();

            document.Info.Title =
                "NDT Register";

            var page =
                document.AddPage();

            var graphics =
                XGraphics.FromPdfPage(page);

            var titleFont =
                new XFont(
                    "Arial",
                    18,
                    XFontStyle.Bold);

            var bodyFont =
                new XFont(
                    "Arial",
                    9,
                    XFontStyle.Regular);

            double y = 40;

            graphics.DrawString(
                "NDT REGISTER",
                titleFont,
                XBrushes.Black,
                new XRect(
                    0,
                    y,
                    page.Width,
                    40),
                XStringFormats.TopCenter);

            y += 50;

            graphics.DrawString(
                $"Project: {package.ProjectName}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 30;

            graphics.DrawString(
                "Method | Result | Inspector | Report No",
                bodyFont,
                XBrushes.DarkBlue,
                40,
                y);

            y += 20;

            foreach (var ndt in
                package.NdtResults
                    ?? Enumerable.Empty<WeldNdtResult>())
            {
                graphics.DrawString(
                    $"{ndt.ReportNumber} | " +
                    $"{ndt.NdtMethod} | " +
                    $"{ndt.Result} | " +
                    $"{ndt.InspectorName}",
                    bodyFont,
                    XBrushes.Black,
                    40,
                    y);

                y += 18;

                if (y > page.Height - 40)
                {
                    page = document.AddPage();

                    graphics =
                        XGraphics.FromPdfPage(
                            page);

                    y = 40;

                    graphics.DrawString(
    "NDT REGISTER",
    titleFont,
    XBrushes.Black,
    new XRect(
        0,
        y,
        page.Width,
        40),
    XStringFormats.TopCenter);

                    y += 50;

                    graphics.DrawString(
                        "Method | Result | Inspector | Report No",
                        bodyFont,
                        XBrushes.DarkBlue,
                        40,
                        y);

                    y += 20;
                }
            }

            Directory.CreateDirectory(exportFolder);

            var filePath =
                Path.Combine(
                    exportFolder,
                    "NdtRegister.pdf");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            document.Save(filePath);

            return filePath;
        }
        public string GenerateWeldRegister(
            TurnoverPackage package,
            string exportFolder)
        {
            if (string.IsNullOrWhiteSpace(
        exportFolder))
            {
                throw new InvalidOperationException(
                    "Export folder is required.");
            }

            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package));
            }

            using var document =
                new PdfDocument();

            document.Info.Title =
                "Weld Register";

            var page =
                document.AddPage();

            var graphics =
                XGraphics.FromPdfPage(page);

            var titleFont =
                new XFont(
                    "Arial",
                    18,
                    XFontStyle.Bold);

            var bodyFont =
                new XFont(
                    "Arial",
                    10,
                    XFontStyle.Regular);

            double y = 40;

            graphics.DrawString(
                "WELD REGISTER",
                titleFont,
                XBrushes.Black,
                new XRect(
                    0,
                    y,
                    page.Width,
                    40),
                XStringFormats.TopCenter);

            y += 50;

            graphics.DrawString(
                $"Project: {package.ProjectName}",
                bodyFont,
                XBrushes.Black,
                40,
                y);

            y += 30;

            graphics.DrawString(
                "Weld Number | Status",
                bodyFont,
                XBrushes.DarkBlue,
                40,
                y);

            y += 20;

            foreach (var weld in
                package.Welds
                    ?? Enumerable.Empty<Weld>())
            {
                graphics.DrawString(
                    $"{weld.WeldNumber} | " +
                    $"{weld.WorkflowStatus}",
                    bodyFont,
                    XBrushes.Black,
                    40,
                    y);

                y += 18;

                if (y > page.Height - 40)
                {
                    page = document.AddPage();

                    graphics =
                        XGraphics.FromPdfPage(
                            page);

                    y = 40;

                    graphics.DrawString(
                        "WELD REGISTER",
                        titleFont,
                        XBrushes.Black,
                        new XRect(
                            0,
                            y,
                            page.Width,
                            40),
                        XStringFormats.TopCenter);

                    y += 50;

                    graphics.DrawString(
                        "Weld Number | Status",
                        bodyFont,
                        XBrushes.DarkBlue,
                        40,
                        y);

                    y += 20;
                }
            }

            Directory.CreateDirectory(exportFolder);

            var filePath =
                Path.Combine(
                    exportFolder,
                    "WeldRegister.pdf");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            document.Save(filePath);

            return filePath;
        }

        public string GenerateRepairRegister(
            TurnoverPackage package,
            string exportFolder)
        {
            if (string.IsNullOrWhiteSpace(
        exportFolder))
            {
                throw new InvalidOperationException(
                    "Export folder is required.");
            }

            if (package == null)
            {
                throw new ArgumentNullException(
                    nameof(package));
            }

            using var document =
                new PdfDocument();

            document.Info.Title =
                "Repair Register";

            var page =
                document.AddPage();

            var graphics =
                XGraphics.FromPdfPage(page);

            var titleFont =
                new XFont(
                    "Arial",
                    18,
                    XFontStyle.Bold);

            var bodyFont =
                new XFont(
                    "Arial",
                    10,
                    XFontStyle.Regular);

            double y = 40;

            graphics.DrawString(
                "REPAIR REGISTER",
                titleFont,
                XBrushes.Black,
                new XRect(
                    0,
                    y,
                    page.Width,
                    40),
                XStringFormats.TopCenter);

            y += 50;

            graphics.DrawString(
                "Repair # | Status | Weld ID",
                bodyFont,
                XBrushes.DarkBlue,
                40,
                y);

            y += 20;

            foreach (var repair in
                package.Repairs
                    ?? Enumerable.Empty<RepairRecord>())
            {
                graphics.DrawString(
                    $"#{repair.RepairNumber} | " +
                    $"{repair.Status} | " +
                    $"{repair.WeldId}",
                    bodyFont,
                    XBrushes.Black,
                    40,
                    y);

                y += 18;

                if (y > page.Height - 40)
                {
                    page = document.AddPage();

                    graphics =
                        XGraphics.FromPdfPage(
                            page);

                    y = 40;

                    graphics.DrawString(
                        "REPAIR REGISTER",
                        titleFont,
                        XBrushes.Black,
                        new XRect(
                            0,
                            y,
                            page.Width,
                            40),
                    XStringFormats.TopCenter);

                    y += 50;

                    graphics.DrawString(
                        "Repair # | Status | Weld ID",
                        bodyFont,
                        XBrushes.DarkBlue,
                        40,
                        y);

                    y += 20;
                }
            }

            Directory.CreateDirectory(exportFolder);

            var filePath =
                    Path.Combine(
                        exportFolder,
                        "RepairRegister.pdf");

            graphics.DrawString(
    $"Generated: {DateTime.Now:g}",
    bodyFont,
    XBrushes.Gray,
    40,
    page.Height - 30);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            document.Save(filePath);

            return filePath;
        }
    }
}
