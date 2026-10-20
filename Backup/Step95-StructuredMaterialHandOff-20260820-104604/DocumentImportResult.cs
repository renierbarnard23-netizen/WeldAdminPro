using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Web.Models.Import;

public class DocumentImportResult
{
    public ImportStatus Status { get; set; } = ImportStatus.Ready;

    public string FileName { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public List<ImportWarning> Warnings { get; set; } = new();

    public Pqr? Pqr { get; set; }
}