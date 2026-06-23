using PdfSharpCore.Pdf.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WeldAdminPro.Core.Reporting.Models;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class PdfExportService
    {
        public void Export(
            WeldDataBook book,
            string path)
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // =====================================
                    // HEADER
                    // =====================================

                    page.Header()
                        .Column(col =>
                        {
                            col.Item()
                                .Text(book.Company.CompanyName)
                                .FontSize(24)
                                .Bold();

                            col.Item()
                                .Text(book.ProjectName)
                                .FontSize(18);

                            col.Item()
                                .Text($"Client: {book.ClientName}");

                            col.Item()
                                .Text(
                                    $"Generated: {book.GeneratedDate:yyyy-MM-dd HH:mm}");
                        });

                    // =====================================
                    // CONTENT
                    // =====================================

                    page.Content()
                        .PaddingVertical(20)
                        .Column(col =>
                        {
                            // =====================================
                            // COVER PAGE
                            // =====================================

                            col.Item()
                                .AlignCenter()
                                .PaddingTop(80)
                                .Text(book.RevisionInfo.DocumentTitle)
                                .FontSize(32)
                                .Bold();

                            col.Item()
                                .PaddingTop(40)
                                .AlignCenter()
                                .Text(book.ProjectName)
                                .FontSize(24);

                            col.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text($"Client: {book.ClientName}")
                                .FontSize(18);

                            col.Item()
                                .PaddingTop(40);

                            col.Item()
                                .Border(1)
                                .Padding(15)
                                .Column(info =>
                                {
                                    info.Item().Text(
                                        $"Project Number: {book.RevisionInfo.ProjectNumber}");

                                    info.Item().Text(
                                        $"Data Book Number: {book.RevisionInfo.DataBookNumber}");

                                    info.Item().Text(
                                        $"Revision: {book.RevisionInfo.Revision}");

                                    info.Item().Text(
                                        $"Prepared By: {book.RevisionInfo.PreparedBy}");

                                    info.Item().Text(
                                        $"Approved By: {book.RevisionInfo.ApprovedBy}");

                                    info.Item().Text(
                                        $"Revision Date: {book.RevisionInfo.RevisionDate:yyyy-MM-dd}");

                                    info.Item().Text(
                                        $"Client Doc No: {book.RevisionInfo.ClientDocumentNumber}");
                                });

                            col.Item()
                                .PaddingTop(50);

                            col.Item()
                                .Text("CONTROLLED QUALITY DOCUMENT")
                                .Bold()
                                .AlignCenter();

                            col.Item()
                                .PageBreak();

                            // =====================================
                            // REVISION HISTORY
                            // =====================================

                            col.Item()
                                .PaddingTop(30);

                            col.Item()
                                .Text("Revision History")
                                .FontSize(18)
                                .Bold();

                            col.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(80);
                                        columns.ConstantColumn(120);
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(120);
                                    });

                                    // HEADER

                                    table.Cell().Text("Rev").Bold();
                                    table.Cell().Text("Date").Bold();
                                    table.Cell().Text("Description").Bold();
                                    table.Cell().Text("Status").Bold();

                                    foreach (var rev in
                                        book.RevisionHistory)
                                    {
                                        table.Cell()
                                            .Text(rev.Revision);

                                        table.Cell()
                                            .Text(
                                                rev.RevisionDate
                                                    .ToString("yyyy-MM-dd"));

                                        table.Cell()
                                            .Text(rev.Description);

                                        table.Cell()
                                            .Text(
                                                rev.Status.ToString());
                                    }
                                });

                            col.Item()
                                .PaddingTop(20);

                            col.Item()
                                .Text(
                                    $"Controlled Copy: {(book.RevisionInfo.IsControlledCopy ? "YES" : "NO")}")
                                .Bold();


                            // =====================================
                            // TABLE OF CONTENTS
                            // =====================================

                            col.Item()
                                .Text("Table of Contents")
                                .FontSize(24)
                                .Bold();

                            foreach (var section in book.Sections)
                            {
                                col.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text(
                                                $"{section.Number}. {section.Title}");

                                        row.ConstantItem(50)
                                            .AlignRight()
                                            .Text(
                                                section.PageNumber.ToString());
                                    });
                            }

                            col.Item()
                                .PageBreak();

                            // =====================================
                            // SUMMARY
                            // =====================================

                            col.Item()
                                .Text("Executive QA Summary")
                                .FontSize(20)
                                .Bold();

                            if (book.RepairStatusChart != null)
                            {
                                col.Item()
                                    .PaddingTop(20);

                                col.Item()
                                    .Text("Repair Status Overview")
                                    .FontSize(18)
                                    .Bold();

                                col.Item()
                                    .Image(book.RepairStatusChart);
                            }

                            // =====================================
                            // ANALYTICS
                            // =====================================

                            col.Item()
                                .PaddingTop(20);

                            col.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    void Row(
                                        string title,
                                        string value)
                                    {
                                        table.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .Text(title);

                                        table.Cell()
                                            .BorderBottom(1)
                                            .Padding(5)
                                            .Text(value);
                                    }

                                    Row(
                                        "Total Welds",
                                        book.Analytics.TotalWelds.ToString());

                                    Row(
                                        "Repairs",
                                        book.Analytics.TotalRepairs.ToString());

                                    Row(
                                        "Repair Rate",
                                        $"{book.Analytics.RepairRate:F1}%");

                                    Row(
                                        "Worst Welder",
                                        book.Analytics.MostFailedWelder);

                                    Row(
                                        "Worst WPS",
                                        book.Analytics.MostFailedWps);
                                });

                            // =====================================
                            // DETAILED WELDS
                            // =====================================

                            col.Item()
                                .PaddingTop(25);

                            col.Item()
                                .Text("Detailed Weld Records")
                                .FontSize(20)
                                .Bold();

                            foreach (var weld in book.Welds)
                            {
                                col.Item()
                                    .PaddingTop(15)
                                    .ShowEntire()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(w =>

                                    {
                                        w.Item()
                                            .Text(
                                                $"Weld: {weld.Weld.WeldNumber}")
                                            .Bold();

                                        w.Item()
                                            .Text(
                                                $"Status: {weld.Weld.Status}");

                                        w.Item()
                                            .Text(
                                                $"NDT Status: {weld.Weld.NdtStatus}");

                                        w.Item()
                                            .Text(
                                                $"Welder: {weld.Weld.WelderNumber}");

                                        w.Item()
                                            .Text(
                                                $"WPS: {weld.Weld.WpsNumber}");
                                        w.Item()
                                            .Text(
                                            $"Repair Count: {weld.Weld.RepairCount}");

                                        w.Item()
                                            .Text(
                                                $"Repair Cycle: {weld.Weld.RepairCycle}");

                                        w.Item()
                                            .Text(
                                                $"Last NDT Result: {weld.Weld.LastNdtResult}");

                                        w.Item()
                                            .PaddingTop(10);

                                        w.Item()
                                            .Text("NDT Results")
                                            .Bold();

                                        foreach (var ndt in weld.NdtResults)
                                        {
                                            w.Item()
                                                .Text(
                                                    $"{ndt.NdtMethod} | {ndt.Result}");
                                        }
                                        // =====================================
                                        // WELD HISTORY
                                        // =====================================

                                        if (weld.History.Any())
                                        {
                                            w.Item()
                                                .PaddingTop(10);

                                            w.Item()
                                                .Text("Weld History")
                                                .Bold();

                                            foreach (var history in weld.History
                                                .OrderBy(x => x.EventDate))
                                            {
                                                w.Item()
                                                    .Text(
                                                        $"{history.EventDate:yyyy-MM-dd HH:mm} | " +
                                                        $"{history.EventType} | " +
                                                        $"{history.Description}");
                                            }
                                        }


                                    }); // END COLUMN

                            } // END WELD LOOP

                            // =====================================
                            // WELDER PERFORMANCE
                            // =====================================

                            col.Item()
                                .PageBreak();

                            col.Item()
                                .Text("Welder Performance")
                                .FontSize(22)
                                .Bold();

                            foreach (var welder
                                in book.WelderPerformance)
                            {
                                col.Item()
                                    .PaddingTop(15)
                                    .Border(1)
                                    .Padding(10)
                                    .Column(w =>
                                    {
                                        w.Item()
                                            .Text(
                                                $"Welder: {welder.WelderNumber}")
                                            .Bold();

                                        w.Item()
                                            .Text(
                                                $"Total Welds: {welder.TotalWelds}");

                                        w.Item()
                                            .Text(
                                                $"Accepted: {welder.AcceptedWelds}");

                                        w.Item()
                                            .Text(
                                                $"Repairs: {welder.Repairs}");

                                        w.Item()
                                            .Text(
                                                $"Repeat Repairs: {welder.RepeatRepairs}");

                                        w.Item()
                                            .Text(
                                                $"Repair Rate: {welder.RepairRate:F1}%");

                                        w.Item()
                                            .Text(
                                                $"Worst WPS: {welder.WorstWps}");
                                    });
                            }

                            // =====================================
                            // ATTACHMENTS
                            // =====================================

                            col.Item()
                                                            .PaddingTop(10)
                                                            .Text("Controlled Attachment Register")
                                                            .FontSize(18)
                                                            .Bold();
                            col.Item()
                                .PaddingTop(10)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // HEADER

                                    table.Header(header =>
                                    {
                                        header.Cell().Border(1).Padding(5)
                                            .Text("Doc No").Bold();

                                        header.Cell().Border(1).Padding(5)
                                            .Text("Title").Bold();

                                        header.Cell().Border(1).Padding(5)
                                            .Text("Rev").Bold();

                                        header.Cell().Border(1).Padding(5)
                                            .Text("Status").Bold();

                                        header.Cell().Border(1).Padding(5)
                                            .Text("Category").Bold();
                                    });

                                    // ROWS

                                    foreach (var attachment
                                        in book.Attachments)
                                    {
                                        table.Cell().Border(1).Padding(5)
                                            .Text(attachment.DocumentNumber);

                                        table.Cell().Border(1).Padding(5)
                                            .Text(attachment.Title);

                                        table.Cell().Border(1).Padding(5)
                                            .Text(attachment.Revision);

                                        table.Cell().Border(1).Padding(5)
                                            .Text(attachment.Status);

                                        table.Cell().Border(1).Padding(5)
                                            .Text(attachment.Category);
                                    }
                                });

                            // =====================================
                            // FOOTER
                            // =====================================

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("WeldAdmin Pro QA System");
                                });

                        }); // END COLUMN

                }); // END PAGE

            }) // END DOCUMENT CREATE

            .GeneratePdf(path);
        }
    }
}
