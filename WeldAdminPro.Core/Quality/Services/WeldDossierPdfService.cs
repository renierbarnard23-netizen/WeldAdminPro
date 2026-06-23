using PdfSharpCore.Pdf.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.IO;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldDossierPdfService
    {
        public void Export(
            WeldDossierExportModel vm)
        {
            var folder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Dossiers");

            Directory.CreateDirectory(
                folder);

            var path =
                Path.Combine(
                    folder,
                    $"{vm.WeldNumber}_Dossier.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .Column(header =>
                {
                    header.Item()
                        .Text("WeldAdmin Pro")
                        .FontSize(28)
                        .Bold();

                    header.Item()
                        .Text($"Weld Dossier - {vm.WeldNumber}")
                        .FontSize(20);

                    header.Item()
                        .PaddingTop(5)
                        .Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                        .FontSize(10)
                        .FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                        });


                    page.Content()
                        .Column(column =>
                        {

                            // =====================================
                            // WELD SUMMARY TABLE
                            // =====================================

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("Weld Summary")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("Weld");
                                        HeaderCell("WPS");
                                        HeaderCell("Welder");
                                        HeaderCell("NDT");
                                        HeaderCell("Repairs");
                                        HeaderCell("Status");
                                    });

                                    // ROWS

                                    foreach (var row in vm.WeldSummary)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.WeldNumber);
                                        Cell(row.Wps);
                                        Cell(row.Welder);
                                        Cell(row.NdtStatus);
                                        Cell(row.Repairs.ToString());
                                        Cell(row.WorkflowStatus);
                                    }
                                });

                            // =====================================
                            // WELD INFO
                            // =====================================

                            column.Item()
                                .PaddingTop(20)
                                .Background(QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("Weld Information")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                    });

                                    void Row(string label, string value)
                                    {
                                        table.Cell()
                                            .Background(
                                                QuestPDF.Helpers.Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .Text(label)
                                            .Bold();

                                        table.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .Text(value);
                                    }

                                        Row("Weld Number", vm.WeldNumber);
                                });


                            // =====================================
                            // NDT HISTORY
                            // =====================================

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("NDT History")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("Method");
                                        HeaderCell("Report");
                                        HeaderCell("Result");
                                        HeaderCell("Technician");
                                        HeaderCell("Date");
                                    });

                                    // ROWS

                                    foreach (var row in vm.NdtRows)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.Method);
                                        Cell(row.ReportNumber);
                                        Cell(row.Result);
                                        Cell(row.Technician);
                                        Cell(row.Date);
                                    }
                                });



                            // =====================================
                            // NCR HISTORY
                            // =====================================

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("NCR History")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("NCR");
                                        HeaderCell("Description");
                                        HeaderCell("Status");
                                        HeaderCell("Corrective Action");
                                        HeaderCell("Closed");
                                    });

                                    // ROWS

                                    foreach (var row in vm.NcrRows)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.NcrNumber);
                                        Cell(row.Description);
                                        Cell(row.Status);
                                        Cell(row.CorrectiveAction);
                                        Cell(row.Closed);
                                    }
                                });

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("Repair History")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("Repair #");
                                        HeaderCell("Reason");
                                        HeaderCell("Repair WPS");
                                        HeaderCell("Welder");
                                        HeaderCell("Result");
                                        HeaderCell("Status");
                                    });

                                    // ROWS

                                    foreach (var row in vm.RepairRows)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.RepairNumber);
                                        Cell(row.Reason);
                                        Cell(row.RepairWps);
                                        Cell(row.Welder);
                                        Cell(row.Result);
                                        Cell(row.Status);
                                    }
                                });

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("Hold Point Signatures")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("Hold Point");
                                        HeaderCell("Approver");
                                        HeaderCell("Role");
                                        HeaderCell("Approved Date");
                                        HeaderCell("Status");
                                    });

                                    // ROWS

                                    foreach (var row in vm.HoldPointRows)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.HoldPoint);
                                        Cell(row.Approver);
                                        Cell(row.Role);
                                        Cell(row.ApprovedDate);
                                        Cell(row.Status);
                                    }
                                });

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("Attachment Index")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("File");
                                        HeaderCell("Category");
                                        HeaderCell("Uploaded By");
                                        HeaderCell("Date");
                                    });

                                    // ROWS

                                    foreach (var row in vm.AttachmentRows)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.FileName);
                                        Cell(row.Category);
                                        Cell(row.UploadedBy);
                                        Cell(row.UploadedDate);
                                    }
                                });

                            column.Item()
                                .PaddingTop(20)
                                .Background(
                                    QuestPDF.Helpers.Colors.Blue.Lighten4)
                                .Padding(10)
                                .Text("QA / Turnover Signoff")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        void HeaderCell(string text)
                                        {
                                            header.Cell()
                                                .Background(
                                                    QuestPDF.Helpers.Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text(text)
                                                .Bold();
                                        }

                                        HeaderCell("Role");
                                        HeaderCell("Name");
                                        HeaderCell("Status");
                                        HeaderCell("Date");
                                        HeaderCell("Signature");
                                    });

                                    // ROWS

                                    foreach (var row in vm.QaSignoffs)
                                    {
                                        void Cell(string text)
                                        {
                                            table.Cell()
                                                .BorderBottom(1)
                                                .Padding(5)
                                                .Text(text);
                                        }

                                        Cell(row.Role);
                                        Cell(row.Name);
                                        Cell(row.Status);
                                        Cell(row.Date);
                                        Cell(row.Signature);
                                    }
                                });



                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated by WeldAdmin Pro QA System");
                        });
                });
            })
            .GeneratePdf(path);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
        }
    }
}
