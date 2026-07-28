namespace WeldAdminPro.Data.Models.OCR;

public class OcrPage
{
    /// <summary>
    /// Page number within the PDF.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// OCR text extracted from this page.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional image path used during OCR.
    /// Useful for debugging.
    /// </summary>
    public string? ImagePath { get; set; }

    public override string ToString()
    {
        return $"Page {PageNumber}";
    }
}