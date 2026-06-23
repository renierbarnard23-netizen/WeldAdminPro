using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.IO;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class ProjectMrbPdfService
    {
        public void Export(
            ProjectMrbExportModel model)
        {
            var folder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "MRB");

            Directory.CreateDirectory(
                folder);

            var path =
                Path.Combine(
                    folder,
                    $"{model.ProjectNumber}_MRB.pdf");

            Document.Create(container =>
            {
                foreach (var weld in model.Welds)
                {
                    container.Page(page =>
                    {
                        page.Margin(40);

                        // HEADER

                        page.Header()
    .BorderBottom(1)
    .PaddingBottom(10)
    .Row(row =>
    {
        // LEFT SIDE

        row.RelativeItem()
            .Column(column =>
            {
                column.Item()
                    .Text("DYNAMIC OPTIONS ENGINEERING")
                    .FontSize(20)
                    .Bold();

                column.Item()
                    .Text("Manufacturing Record Book")
                    .FontSize(16);

                column.Item()
                    .Text(
                        $"Project: {model.ProjectNumber} - {model.ProjectName}");

                column.Item()
                    .Text(
                        $"Weld: {weld.WeldNumber}");
            });

        // RIGHT SIDE

        row.ConstantItem(180)
            .Column(column =>
            {
                column.Item()
                    .AlignRight()
                    .Text("Document No: MRB-001");

                column.Item()
                    .AlignRight()
                    .Text("Revision: 0");

                column.Item()
                    .AlignRight()
                    .Text(
                        $"Date: {DateTime.Now:yyyy-MM-dd}");
            });
    });

                        // CONTENT

                        page.Content()
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingTop(20)
                                    .Background(
                                        Colors.Blue.Lighten4)
                                    .Padding(10)
                                    .Text("Weld Summary")
                                    .FontSize(16)
                                    .Bold();

                                column.Item()
                                    .Text(
                                        $"Weld Number: {weld.WeldNumber}");

                                // ============================
                                // NDT TABLE
                                // ============================

                                column.Item()
                                    .PaddingTop(20)
                                    .Text("NDT History")
                                    .FontSize(16)
                                    .Bold();

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        // HEADER

                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Method")
                                                .Bold();

                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Result")
                                                .Bold();

                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Technician")
                                                .Bold();

                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Date")
                                                .Bold();
                                        });

                                        // ROWS

                                        foreach (var row in weld.NdtRows)
                                        {
                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Method);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Result);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Technician);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Date);
                                        }
                                    });

                                // ============================
                                // NCR TABLE
                                // ============================

                                column.Item()
                                    .PaddingTop(20)
                                    .Text("NCR History")
                                    .FontSize(16)
                                    .Bold();

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        // HEADER

                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("NCR Number")
                                                .Bold();

                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Status")
                                                .Bold();

                                            header.Cell()
                                                .Background(Colors.Grey.Lighten2)
                                                .Padding(5)
                                                .Text("Description")
                                                .Bold();
                                        });

                                        // ROWS

                                        foreach (var row in weld.NcrRows)
                                        {
                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.NcrNumber);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Status);

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(row.Description);
                                        }
                                    });

                                // ============================
                                // INCLUDED DOCUMENTS
                                // ============================

                                column.Item()
                                    .PaddingTop(20)
                                    .Text("Included Documents")
                                    .FontSize(16)
                                    .Bold();

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(30);
                                            columns.RelativeColumn();
                                        });

                                        void AddDocument(string name)
                                        {
                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text("✓");

                                            table.Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(name);
                                        }

                                        AddDocument("Approved WPS");
                                        AddDocument("Supporting PQR");
                                        AddDocument("Welder Qualification");
                                        AddDocument("NDT Report");
                                        AddDocument("Repair History");
                                        AddDocument("NCR Records");
                                        AddDocument("Material Traceability");
                                        AddDocument("Heat Certificates");
                                    });
                            });

                        

                        // FOOTER

                        page.Footer()
                            .BorderTop(1)
                            .PaddingTop(5)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                   .Text($"Generated by WeldAdmin Pro | {DateTime.Now:yyyy-MM-dd HH:mm}");

                                row.ConstantItem(120)
                                   .AlignRight()
                                   .Text(text =>
                                    {
                                        text.CurrentPageNumber();
                                        text.Span(" / ");
                                        text.TotalPages();
                                    });
                            });
                    });
        }
}).GeneratePdf(path);
            // Open the generated PDF
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }
}
