namespace WeldAdminPro.Data.Models.OCR;

public class OcrRecognitionContext
{
    public OcrDocument Document { get; }

    public OcrRecognitionContext(OcrDocument document)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Complete OCR document.
    /// </summary>
    public string FullText => Document.FullText;

    /// <summary>
    /// Page 1 only.
    /// </summary>
    public string FirstPageText => Document.FirstPageText;

    /// <summary>
    /// Pages 2+
    /// </summary>
    public string RemainingPagesText => Document.RemainingPagesText;

    /// <summary>
    /// Convenience property.
    /// </summary>
    public OcrPage? FirstPage => Document.FirstPage;
}