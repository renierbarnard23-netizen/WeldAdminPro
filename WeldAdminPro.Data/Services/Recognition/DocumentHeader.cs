namespace WeldAdminPro.Data.Services.Recognition;

/// <summary>
/// Represents the header information extracted from the first page
/// of a PQR or WPS document.
/// </summary>
public class DocumentHeader
{
    /// <summary>
    /// Procedure Qualification Record Number.
    /// Example: PQR SA310
    /// </summary>
    public string PqrNumber { get; set; } = string.Empty;

    /// <summary>
    /// Welding Procedure Specification Number.
    /// Example: WPS SA310
    /// </summary>
    public string WpsNumber { get; set; } = string.Empty;

    /// <summary>
    /// Applicable construction or qualification code.
    /// Example: ASME Section IX
    /// </summary>
    public string CodeStandard { get; set; } = string.Empty;

    /// <summary>
    /// Document revision.
    /// Example: Rev 0
    /// </summary>
    public string Revision { get; set; } = string.Empty;

    /// <summary>
    /// Document date.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Company or customer designation if present.
    /// </summary>
    public string Designation { get; set; } = string.Empty;
}