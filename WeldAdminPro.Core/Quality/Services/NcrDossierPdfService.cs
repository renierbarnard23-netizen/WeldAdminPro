using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services;

public class NcrDossierPdfService
{
    public byte[] Generate(
        NcrDossierExportModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(model.Ncr);

        QuestPDF.Settings.License =
            LicenseType.Community;

        return Document
            .Create(container =>
            {
                ComposeSummaryPage(
                    container,
                    model);

                ComposeCreationPage(
                    container,
                    model);

                ComposeInvestigationPage(
                    container,
                    model);

                ComposeDispositionPage(
                    container,
                    model);

                ComposeVerificationPage(
                    container,
                    model);

                ComposeAuditTrailPage(
                    container,
                    model);
            })
            .GeneratePdf();
    }


    // =====================================================
    // PAGE 1 - CONTROLLED DOCUMENT SUMMARY
    // =====================================================

    private static void ComposeSummaryPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        var ncr = model.Ncr;

        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "NON-CONFORMANCE REPORT");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(12);

                    column.Item()
                        .Background(Colors.Blue.Lighten4)
                        .Padding(12)
                        .Column(header =>
                        {
                            header.Item()
                                .Text(ncr.NcrNumber)
                                .FontSize(22)
                                .Bold();

                            header.Item()
                                .Text(
                                    $"Status: {FormatStatus(ncr.Status.ToString())}")
                                .FontSize(14)
                                .SemiBold();
                        });

                    column.Item()
                        .PaddingTop(5)
                        .Text("Document Summary")
                        .FontSize(16)
                        .Bold();

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            AddRow(
                                table,
                                "NCR Number",
                                ncr.NcrNumber);

                            AddRow(
                                table,
                                "Status",
                                FormatStatus(
                                    ncr.Status.ToString()));

                            AddRow(
                                table,
                                "Category",
                                ResolveCategory(ncr));

                            AddRow(
                                table,
                                "Welding Related",
                                YesNo(
                                    ncr.IsWeldingRelated));

                            AddRow(
                                table,
                                "Associated Weld",
                                ncr.WeldId.HasValue
                                    ? Safe(ncr.WeldNumber)
                                    : "Not applicable");

                            AddRow(
                                table,
                                "Raised By",
                                Safe(ncr.RaisedBy));

                            AddRow(
                                table,
                                "Raised Date",
                                FormatDate(
                                    ncr.RaisedDate));

                            AddRow(
                                table,
                                "Assigned To",
                                Safe(ncr.AssignedTo));

                            AddRow(
                                table,
                                "Due Date",
                                FormatDate(
                                    ncr.DueDate));

                            AddRow(
                                table,
                                "Final / Closed",
                                YesNo(ncr.IsClosed));
                        });

                    SectionHeading(
                        column,
                        "Non-Conformance Description");

                    ValueBox(
                        column,
                        ncr.Description);

                    if (ncr.IsClosed)
                    {
                        SectionHeading(
                            column,
                            "Closure");

                        column.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                });

                                AddRow(
                                    table,
                                    "Closed By",
                                    Safe(ncr.ClosedBy));

                                AddRow(
                                    table,
                                    "Closed Date",
                                    FormatDate(
                                        ncr.ClosedDate));
                            });
                    }
                    else
                    {
                        column.Item()
                            .PaddingTop(10)
                            .Background(Colors.Orange.Lighten4)
                            .Padding(10)
                            .Text(
                                "INTERIM NCR RECORD - workflow is not yet complete.")
                            .Bold();
                    }
                });
        });
    }


    // =====================================================
    // PAGE 2 - NCR CREATION
    // =====================================================

    private static void ComposeCreationPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        var ncr = model.Ncr;

        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "NCR CREATION");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Original NCR Details");

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            AddRow(
                                table,
                                "NCR Number",
                                ncr.NcrNumber);

                            AddRow(
                                table,
                                "Category",
                                ResolveCategory(ncr));

                            AddRow(
                                table,
                                "Raised By",
                                Safe(ncr.RaisedBy));

                            AddRow(
                                table,
                                "Raised Date",
                                FormatDate(
                                    ncr.RaisedDate));

                            AddRow(
                                table,
                                "Assigned To",
                                Safe(ncr.AssignedTo));

                            AddRow(
                                table,
                                "Due Date",
                                FormatDate(
                                    ncr.DueDate));

                            AddRow(
                                table,
                                "Welding Related",
                                YesNo(
                                    ncr.IsWeldingRelated));

                            AddRow(
                                table,
                                "Associated Weld",
                                ncr.WeldId.HasValue
                                    ? Safe(ncr.WeldNumber)
                                    : "Not applicable");
                        });

                    SectionHeading(
                        column,
                        "Description");

                    ValueBox(
                        column,
                        ncr.Description);

                    var created =
                        model.History
                            .FirstOrDefault(x =>
                                x.Action == "Created");

                    if (created != null)
                    {
                        SectionHeading(
                            column,
                            "Creation Audit Record");

                        AuditEntry(
                            column,
                            created);
                    }
                });
        });
    }


    // =====================================================
    // PAGE 3 - INVESTIGATION
    // =====================================================

    private static void ComposeInvestigationPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        var ncr = model.Ncr;

        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "INVESTIGATION & CORRECTIVE ACTION");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Root Cause");

                    ValueBox(
                        column,
                        ncr.RootCause);

                    SectionHeading(
                        column,
                        "Corrective Action");

                    ValueBox(
                        column,
                        ncr.CorrectiveAction);

                    SectionHeading(
                        column,
                        "Preventive Action");

                    ValueBox(
                        column,
                        ncr.PreventiveAction);

                    var investigationHistory =
                        model.History
                            .Where(x =>
                                x.ToStatus.ToString()
                                    .Contains(
                                        "Investigation",
                                        StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    if (investigationHistory.Count > 0)
                    {
                        SectionHeading(
                            column,
                            "Investigation Workflow History");

                        foreach (var entry in
                                 investigationHistory)
                        {
                            AuditEntry(
                                column,
                                entry);
                        }
                    }
                });
        });
    }


    // =====================================================
    // PAGE 4 - DISPOSITION / CUSTOMER APPROVAL
    // =====================================================

    private static void ComposeDispositionPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        var ncr = model.Ncr;

        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "DISPOSITION & APPROVAL");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Disposition");

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            AddRow(
                                table,
                                "Disposition",
                                ncr.DispositionType
                                    ?.ToString()
                                    ?? "Not yet recorded");

                            AddRow(
                                table,
                                "Approved By",
                                Safe(
                                    ncr.DispositionApprovedBy));

                            AddRow(
                                table,
                                "Approval Date",
                                FormatDate(
                                    ncr.DispositionApprovedDate));

                            AddRow(
                                table,
                                "Customer Approval Required",
                                YesNo(
                                    ncr.RequiresCustomerApproval));

                            AddRow(
                                table,
                                "Customer Approved",
                                ncr.RequiresCustomerApproval
                                    ? YesNo(
                                        ncr.CustomerApproved)
                                    : "Not applicable");

                            AddRow(
                                table,
                                "Customer Approval Reference",
                                ncr.RequiresCustomerApproval
                                    ? Safe(
                                        ncr.CustomerApprovalReference)
                                    : "Not applicable");
                        });

                    var dispositionHistory =
                        model.History
                            .Where(x =>
                                x.Action.Contains(
                                    "Disposition",
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    if (dispositionHistory.Count > 0)
                    {
                        SectionHeading(
                            column,
                            "Disposition Audit Record");

                        foreach (var entry in
                                 dispositionHistory)
                        {
                            AuditEntry(
                                column,
                                entry);
                        }
                    }
                });
        });
    }


    // =====================================================
    // PAGE 5 - VERIFICATION / CLOSURE
    // =====================================================

    private static void ComposeVerificationPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        var ncr = model.Ncr;

        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "VERIFICATION & CLOSURE");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Verification");

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            AddRow(
                                table,
                                "Verified By",
                                Safe(
                                    ncr.VerificationBy));

                            AddRow(
                                table,
                                "Verification Date",
                                FormatDate(
                                    ncr.VerificationDate));

                            AddRow(
                                table,
                                "Current Status",
                                FormatStatus(
                                    ncr.Status.ToString()));
                        });

                    SectionHeading(
                        column,
                        "Closure");

                    column.Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            AddRow(
                                table,
                                "Closed",
                                YesNo(
                                    ncr.IsClosed));

                            AddRow(
                                table,
                                "Closed By",
                                Safe(
                                    ncr.ClosedBy));

                            AddRow(
                                table,
                                "Closed Date",
                                FormatDate(
                                    ncr.ClosedDate));
                        });

                    var verificationHistory =
                        model.History
                            .Where(x =>
                                x.Action.Contains(
                                    "Verification",
                                    StringComparison.OrdinalIgnoreCase) ||
                                x.Action.Equals(
                                    "Closed",
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    if (verificationHistory.Count > 0)
                    {
                        SectionHeading(
                            column,
                            "Verification / Closure Audit");

                        foreach (var entry in
                                 verificationHistory)
                        {
                            AuditEntry(
                                column,
                                entry);
                        }
                    }
                });
        });
    }


    // =====================================================
    // PAGE 6+ - COMPLETE AUDIT TRAIL
    // =====================================================

    private static void ComposeAuditTrailPage(
        IDocumentContainer container,
        NcrDossierExportModel model)
    {
        container.Page(page =>
        {
            ConfigurePage(
                page,
                model,
                "COMPLETE WORKFLOW AUDIT TRAIL");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(8);

                    if (model.History.Count == 0)
                    {
                        ValueBox(
                            column,
                            "No workflow history is available.");

                        return;
                    }

                    foreach (var entry in
                             model.History
                                 .OrderBy(x =>
                                     x.PerformedDate))
                    {
                        AuditEntry(
                            column,
                            entry);
                    }
                });
        });
    }


    // =====================================================
    // COMMON PAGE
    // =====================================================

    private static void ConfigurePage(
        PageDescriptor page,
        NcrDossierExportModel model,
        string title)
    {
        page.Size(PageSizes.A4);
        page.Margin(35);

        page.DefaultTextStyle(
            style =>
                style.FontSize(10));

        page.Header()
            .Column(column =>
            {
                column.Item()
                    .Text("WELDADMIN PRO")
                    .FontSize(9)
                    .Bold()
                    .FontColor(
                        Colors.Grey.Darken2);

                column.Item()
                    .Text(title)
                    .FontSize(18)
                    .Bold();

                column.Item()
                    .PaddingTop(3)
                    .Text(
                        $"NCR: {model.Ncr.NcrNumber}")
                    .FontSize(10);

                column.Item()
                    .PaddingTop(5)
                    .LineHorizontal(1)
                    .LineColor(
                        Colors.Grey.Lighten1);
            });

        page.Footer()
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(text =>
                    {
                        text.Span(
                            $"Generated {model.GeneratedDate:yyyy-MM-dd HH:mm}");

                        if (!string.IsNullOrWhiteSpace(
                                model.GeneratedBy))
                        {
                            text.Span(
                                $" by {model.GeneratedBy}");
                        }
                    });

                row.ConstantItem(100)
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
    }


    // =====================================================
    // COMMON COMPONENTS
    // =====================================================

    private static void SectionHeading(
        ColumnDescriptor column,
        string title)
    {
        column.Item()
            .PaddingTop(8)
            .Background(
                Colors.Blue.Lighten4)
            .Padding(8)
            .Text(title)
            .FontSize(13)
            .Bold();
    }

    private static void ValueBox(
        ColumnDescriptor column,
        string? value)
    {
        column.Item()
            .Border(1)
            .BorderColor(
                Colors.Grey.Lighten1)
            .Padding(10)
            .Text(Safe(value));
    }

    private static void AddRow(
        TableDescriptor table,
        string label,
        string? value)
    {
        table.Cell()
            .Background(
                Colors.Grey.Lighten3)
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten1)
            .Padding(6)
            .Text(label)
            .Bold();

        table.Cell()
            .BorderBottom(1)
            .BorderColor(
                Colors.Grey.Lighten1)
            .Padding(6)
            .Text(Safe(value));
    }

    private static void AuditEntry(
        ColumnDescriptor column,
        NcrWorkflowHistoryEntry entry)
    {
        column.Item()
            .Border(1)
            .BorderColor(
                Colors.Grey.Lighten1)
            .Padding(8)
            .Column(inner =>
            {
                inner.Item()
                    .Text(entry.Action)
                    .Bold();

                inner.Item()
                    .Text(
                        $"{entry.PerformedDate:yyyy-MM-dd HH:mm} | " +
                        $"{Safe(entry.PerformedBy)}");

                inner.Item()
                    .Text(
                        $"Status: " +
                        $"{FormatNullableStatus(entry.FromStatus)} -> " +
                        $"{FormatStatus(entry.ToStatus.ToString())}");

                if (!string.IsNullOrWhiteSpace(
                        entry.Details))
                {
                    inner.Item()
                        .PaddingTop(4)
                        .Text(entry.Details);
                }
            });
    }


    // =====================================================
    // FORMATTERS
    // =====================================================

    private static string ResolveCategory(
        NcrRecord ncr)
    {
        if (string.Equals(
                ncr.Category,
                "Other",
                StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(
                ncr.CustomReason))
        {
            return $"Other - {ncr.CustomReason}";
        }

        return Safe(ncr.Category);
    }

    private static string Safe(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Not recorded"
            : value.Trim();
    }

    private static string YesNo(
        bool value)
    {
        return value
            ? "Yes"
            : "No";
    }

    private static string FormatDate(
        DateTime value)
    {
        return value == default
            ? "Not recorded"
            : value.ToString(
                "yyyy-MM-dd HH:mm");
    }

    private static string FormatDate(
        DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToString(
                "yyyy-MM-dd HH:mm")
            : "Not recorded";
    }

    private static string FormatNullableStatus(
        WeldAdminPro.Core.Quality.Enums.NcrStatus? status)
    {
        return status.HasValue
            ? FormatStatus(
                status.Value.ToString())
            : "Initial";
    }

    private static string FormatStatus(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Not recorded";
        }

        return string.Concat(
            value.Select(
                (character, index) =>
                    index > 0 &&
                    char.IsUpper(character)
                        ? " " + character
                        : character.ToString()));
    }
}