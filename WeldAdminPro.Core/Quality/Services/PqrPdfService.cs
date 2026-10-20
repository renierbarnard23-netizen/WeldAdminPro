using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality.Services;

public class PqrPdfService
{
    public byte[] Generate(
        Pqr pqr,
        string companyName = "Company not recorded",
        string generatedBy = "")
    {
        ArgumentNullException.ThrowIfNull(pqr);

        QuestPDF.Settings.License =
            LicenseType.Community;

        return Document
            .Create(container =>
            {
                ComposeCoverAndSummary(
                    container,
                    pqr,
                    companyName,
                    generatedBy);

                ComposeBaseMetalAndJointPage(
                    container,
                    pqr,
                    companyName,
                    generatedBy);

                ComposeWeldingVariablesPage(
                    container,
                    pqr,
                    companyName,
                    generatedBy);

                ComposeQualificationPage(
                    container,
                    pqr,
                    companyName,
                    generatedBy);
            })
            .GeneratePdf();
    }

    // ============================================================
    // PAGE 1 — CONTROLLED DOCUMENT SUMMARY
    // ============================================================

    private static void ComposeCoverAndSummary(
        IDocumentContainer container,
        Pqr pqr,
        string companyName,
        string generatedBy)
    {
        container.Page(page =>
        {
            ConfigurePage(
                page,
                pqr,
                companyName,
                generatedBy,
                "PROCEDURE QUALIFICATION RECORD");

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
                                .Text("PROCEDURE QUALIFICATION RECORD")
                                .FontSize(20)
                                .Bold();

                            header.Item()
                                .PaddingTop(4)
                                .Text(
                                    $"PQR: {Safe(pqr.PqrNumber)}")
                                .FontSize(14)
                                .SemiBold();

                            header.Item()
                                .PaddingTop(2)
                                .Text(
                                    $"Revision: {pqr.Revision}")
                                .FontSize(11);
                        });

                    SectionHeading(
                        column,
                        "Document Control");

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
                                "Company",
                                companyName);

                            AddRow(
                                table,
                                "PQR Number",
                                pqr.PqrNumber);

                            AddRow(
                                table,
                                "Revision",
                                pqr.Revision.ToString());

                            AddRow(
                                table,
                                "Qualification Date",
                                FormatDate(pqr.QualificationDate));

                            AddRow(
                                table,
                                "Qualified By",
                                pqr.QualifiedBy);

                            AddRow(
                                table,
                                "Standard",
                                pqr.Standard);

                            AddRow(
                                table,
                                "WPS Reference",
                                !string.IsNullOrWhiteSpace(
                                    pqr.WpsReferenceNumber)
                                    ? pqr.WpsReferenceNumber
                                    : "Not recorded");

                            AddRow(
                                table,
                                "Approval Status",
                                pqr.IsApproved
                                    ? "APPROVED"
                                    : "NOT APPROVED");

                            AddRow(
                                table,
                                "Approved By",
                                pqr.ApprovedBy);

                            AddRow(
                                table,
                                "Approved On",
                                FormatDate(pqr.ApprovedOn));

                            AddRow(
                                table,
                                "Locked",
                                YesNo(pqr.IsLocked));

                            AddRow(
                                table,
                                "Active Revision",
                                YesNo(pqr.IsActive));
                        });

                    SectionHeading(
                        column,
                        "Qualification Summary");

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
                                "Welding Process",
                                pqr.Process);

                            AddRow(
                                table,
                                "Welding Type",
                                pqr.WeldingType);

                            AddRow(
                                table,
                                "Position",
                                !string.IsNullOrWhiteSpace(
                                    pqr.QualifiedPosition)
                                    ? pqr.QualifiedPosition
                                    : pqr.WeldingPosition);

                            AddRow(
                                table,
                                "Material Group",
                                pqr.MaterialGroup);

                            AddRow(
                                table,
                                "P-Number",
                                pqr.PNumber);

                            AddRow(
                                table,
                                "Qualified P-Number Range",
                                pqr.QualifiedPNumberRange);

                            AddRow(
                                table,
                                "Thickness Tested",
                                FormatNumber(
                                    pqr.ThicknessTested));

                            AddRow(
                                table,
                                "Qualified Thickness Range",
                                $"{FormatNumber(pqr.ThicknessQualifiedMin)} - " +
                                $"{FormatNumber(pqr.ThicknessQualifiedMax)}");

                            AddRow(
                                table,
                                "Pipe",
                                YesNo(pqr.IsPipe));

                            AddRow(
                                table,
                                "Diameter Tested",
                                FormatNumber(pqr.DiameterTested));

                            AddRow(
                                table,
                                "Qualified Diameter Range",
                                $"{FormatNumber(pqr.DiameterMin)} - " +
                                $"{FormatNumber(pqr.DiameterMax)}");
                        });
                });
        });
    }

    // ============================================================
    // PAGE 2 — BASE METAL / JOINT
    // ============================================================

    private static void ComposeBaseMetalAndJointPage(
        IDocumentContainer container,
        Pqr pqr,
        string companyName,
        string generatedBy)
    {
        container.Page(page =>
        {
            ConfigurePage(
                page,
                pqr,
                companyName,
                generatedBy,
                "BASE METAL & JOINT DESIGN");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Base Metal — Test Coupon 1");

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
                                "Material",
                                pqr.BaseMaterial1);

                            AddRow(
                                table,
                                "Specification",
                                pqr.BaseMaterial1Specification);

                            AddRow(
                                table,
                                "Grade",
                                pqr.BaseMaterial1Grade);

                            AddRow(
                                table,
                                "UNS",
                                pqr.BaseMaterial1UNS);
                        });

                    SectionHeading(
                        column,
                        "Base Metal — Test Coupon 2");

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
                                "Material",
                                pqr.BaseMaterial2);

                            AddRow(
                                table,
                                "Specification",
                                pqr.BaseMaterial2Specification);

                            AddRow(
                                table,
                                "Grade",
                                pqr.BaseMaterial2Grade);

                            AddRow(
                                table,
                                "UNS",
                                pqr.BaseMaterial2UNS);
                        });

                    SectionHeading(
                        column,
                        "Joint Design");

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
                                "Joint Type",
                                pqr.JointType);

                            AddRow(
                                table,
                                "Joint Design",
                                pqr.JointDesign);

                            AddRow(
                                table,
                                "Edge Preparation",
                                pqr.EdgePreparation);

                            AddRow(
                                table,
                                "Surface Preparation",
                                pqr.SurfacePreparation);

                            AddRow(
                                table,
                                "Groove Angle",
                                FormatNumber(pqr.GrooveAngle));

                            AddRow(
                                table,
                                "Root Face",
                                FormatNumber(pqr.RootFace));

                            AddRow(
                                table,
                                "Root Gap",
                                FormatNumber(pqr.RootGap));

                            AddRow(
                                table,
                                "Groove Radius",
                                FormatNumber(pqr.GrooveRadius));

                            AddRow(
                                table,
                                "Misalignment",
                                FormatNumber(pqr.Misalignment));

                            AddRow(
                                table,
                                "Backing",
                                pqr.Backing);

                            AddRow(
                                table,
                                "Backing Type",
                                pqr.BackingType);

                            AddRow(
                                table,
                                "Back Gouging",
                                pqr.BackGouging);
                        });
                });
        });
    }

    // ============================================================
    // PAGE 3 — WELDING VARIABLES
    // ============================================================

    private static void ComposeWeldingVariablesPage(
        IDocumentContainer container,
        Pqr pqr,
        string companyName,
        string generatedBy)
    {
        container.Page(page =>
        {
            ConfigurePage(
                page,
                pqr,
                companyName,
                generatedBy,
                "WELDING VARIABLES");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Filler Metal");

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
                                "Filler Material",
                                pqr.FillerMaterial);

                            AddRow(
                                table,
                                "Filler Classification",
                                pqr.FillerClassification);

                            AddRow(
                                table,
                                "F-Number",
                                pqr.FNumber);

                            AddRow(
                                table,
                                "A-Number",
                                pqr.ANumber);
                        });

                    SectionHeading(
                        column,
                        "Electrical / Welding Parameters");

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
                                "Current Type",
                                pqr.CurrentType);

                            AddRow(
                                table,
                                "Polarity",
                                pqr.Polarity);

                            AddRow(
                                table,
                                "Amperage",
                                FormatNumber(pqr.AmpsUsed));

                            AddRow(
                                table,
                                "Voltage",
                                FormatNumber(pqr.VoltsUsed));

                            AddRow(
                                table,
                                "Travel Speed",
                                FormatNumber(pqr.TravelSpeed));

                            AddRow(
                                table,
                                "Heat Input",
                                FormatNumber(pqr.HeatInput));

                            AddRow(
                                table,
                                "Transfer Mode",
                                pqr.TransferMode);

                            AddRow(
                                table,
                                "Progression",
                                pqr.Progression);
                        });

                    SectionHeading(
                        column,
                        "Shielding / Backing Gas");

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
                                "Shielding Gas",
                                pqr.ShieldingGas);

                            AddRow(
                                table,
                                "Shielding Gas Flow Rate",
                                FormatNumber(pqr.GasFlowRate));

                            AddRow(
                                table,
                                "Backing Gas",
                                pqr.BackingGas);

                            AddRow(
                                table,
                                "Backing Gas Flow Rate",
                                FormatNumber(pqr.BackingGasFlowRate));

                            AddRow(
                                table,
                                "Gas Type",
                                pqr.GasType);
                        });

                    SectionHeading(
                        column,
                        "Temperature / Heat Treatment");

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
                                "Preheat",
                                FormatNumber(pqr.Preheat));

                            AddRow(
                                table,
                                "Preheat Temperature",
                                pqr.PreheatTemperature);

                            AddRow(
                                table,
                                "Interpass",
                                FormatNumber(pqr.Interpass));

                            AddRow(
                                table,
                                "Preheat Notes",
                                pqr.PreheatNotes);

                            AddRow(
                                table,
                                "PWHT Performed",
                                YesNo(pqr.PwhtPerformed));

                            AddRow(
                                table,
                                "Post Weld Heat Treatment",
                                pqr.PostWeldHeatTreatment);
                        });
                });
        });
    }

    // ============================================================
    // PAGE 4 — QUALIFICATION
    // ============================================================

    private static void ComposeQualificationPage(
        IDocumentContainer container,
        Pqr pqr,
        string companyName,
        string generatedBy)
    {
        container.Page(page =>
        {
            ConfigurePage(
                page,
                pqr,
                companyName,
                generatedBy,
                "QUALIFICATION & CONTROL");

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);

                    SectionHeading(
                        column,
                        "Qualification Limits");

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
                                "Material Group",
                                pqr.MaterialGroup);

                            AddRow(
                                table,
                                "P-Number",
                                pqr.PNumber);

                            AddRow(
                                table,
                                "P-Number 2",
                                pqr.PNumber2);

                            AddRow(
                                table,
                                "Qualified P-Number Range",
                                pqr.QualifiedPNumberRange);

                            AddRow(
                                table,
                                "Qualified Position",
                                pqr.QualifiedPosition);

                            AddRow(
                                table,
                                "Thickness Minimum",
                                FormatNumber(
                                    pqr.ThicknessQualifiedMin));

                            AddRow(
                                table,
                                "Thickness Maximum",
                                FormatNumber(
                                    pqr.ThicknessQualifiedMax));

                            AddRow(
                                table,
                                "Diameter Minimum",
                                FormatNumber(
                                    pqr.DiameterMin));

                            AddRow(
                                table,
                                "Diameter Maximum",
                                FormatNumber(
                                    pqr.DiameterMax));
                        });

                    SectionHeading(
                        column,
                        "Referenced WPS");

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
                                "WPS Reference Number",
                                pqr.WpsReferenceNumber);

                            AddRow(
                                table,
                                "WPS Record",
                                pqr.WpsId.HasValue
                                    ? pqr.WpsId.Value.ToString()
                                    : "Not linked");
                        });

                    SectionHeading(
                        column,
                        "Approval / Release Control");

                    column.Item()
                        .Background(
                            pqr.IsApproved
                                ? Colors.Green.Lighten4
                                : Colors.Orange.Lighten4)
                        .Padding(12)
                        .Column(status =>
                        {
                            status.Item()
                                .Text(
                                    pqr.IsApproved
                                        ? "APPROVED PQR"
                                        : "PQR NOT APPROVED")
                                .FontSize(16)
                                .Bold();

                            status.Item()
                                .PaddingTop(5)
                                .Text(
                                    pqr.IsApproved
                                        ? $"Approved by {Safe(pqr.ApprovedBy)} " +
                                          $"on {FormatDate(pqr.ApprovedOn)}"
                                        : "This PQR has not been approved.");
                        });

                    column.Item()
                        .PaddingTop(20)
                        .Text(
                            "Controlled Quality Master Record")
                        .FontSize(11)
                        .Bold();

                    column.Item()
                        .Text(
                            "This generated document represents the structured " +
                            "PQR record held by WeldAdmin Pro. Where an original " +
                            "source document exists, it is retained separately " +
                            "as the original document.")
                        .FontSize(9);
                });
        });
    }

    // ============================================================
    // COMMON PAGE CONFIGURATION
    // ============================================================

    private static void ConfigurePage(
        PageDescriptor page,
        Pqr pqr,
        string companyName,
        string generatedBy,
        string title)
    {
        page.Size(PageSizes.A4);
        page.Margin(35);

        page.DefaultTextStyle(
            style =>
                style.FontSize(9));

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
                        $"PQR: {Safe(pqr.PqrNumber)} | " +
                        $"Company: {Safe(companyName)}")
                    .FontSize(9);

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
                            $"Generated {DateTime.Now:yyyy-MM-dd HH:mm}");

                        if (!string.IsNullOrWhiteSpace(
                                generatedBy))
                        {
                            text.Span(
                                $" by {generatedBy}");
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

    // ============================================================
    // COMMON COMPONENTS
    // ============================================================

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
            .FontSize(12)
            .Bold();
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

    // ============================================================
    // FORMATTERS
    // ============================================================

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
            : value.ToString("yyyy-MM-dd");
    }

    private static string FormatDate(
        DateTime? value)
    {
        return value.HasValue
            ? value.Value.ToString("yyyy-MM-dd")
            : "Not recorded";
    }

    private static string FormatNumber(
        double value)
    {
        return value == 0
            ? "Not recorded"
            : value.ToString("0.##");
    }
}
