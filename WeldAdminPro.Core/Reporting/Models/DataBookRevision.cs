namespace WeldAdminPro.Core.Reporting.Models;

using WeldAdminPro.Core.Reporting.Enums;

public class DataBookRevision
{
    public string Revision { get; set; } = "0";

    public string PreparedBy { get; set; } = "";

    public string ApprovedBy { get; set; } = "";

    public string ProjectNumber { get; set; } = "";

    public string DataBookNumber { get; set; } = "";

    public string DocumentTitle { get; set; } =
        "Welding Quality Data Book";

    public string ClientDocumentNumber { get; set; } = "";

    public DateTime RevisionDate { get; set; } =
        DateTime.Now;

    public string Notes { get; set; } = "";

    public DocumentStatusType Status { get; set; } = DocumentStatusType.Draft;

    public bool IsControlledCopy { get; set; } = true;

    public string RevisionNotes { get; set; } = "";

    public string CheckedBy { get; set; } = "";

}
