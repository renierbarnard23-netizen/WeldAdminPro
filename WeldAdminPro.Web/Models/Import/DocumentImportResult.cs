using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Web.Models.Import;

public class DocumentImportResult
{
    public ImportStatus Status { get; set; } = ImportStatus.Ready;

    public string FileName { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;

    public double Confidence { get; set; }
    // =========================================================
    // STRUCTURED MATERIAL RECOGNITION
    // Preserved from RecognitionEngine so the Web import layer
    // can resolve the authoritative BaseMaterial.
    // =========================================================

    public string MaterialSpecification { get; set; } = string.Empty;

    public string MaterialGrade { get; set; } = string.Empty;

    public string MaterialUNS { get; set; } = string.Empty;

    public string MaterialPNumber { get; set; } = string.Empty;

    public List<ImportWarning> Warnings { get; set; } = new();

    public Pqr? Pqr { get; set; }
}
