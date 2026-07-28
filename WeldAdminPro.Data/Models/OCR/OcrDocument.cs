using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldAdminPro.Data.Models.OCR;

public class OcrDocument
{
    public List<OcrPage> Pages { get; } = new();

    public int PageCount => Pages.Count;

    public OcrPage? FirstPage =>
        Pages.FirstOrDefault();

    public OcrPage? LastPage =>
        Pages.LastOrDefault();

    /// <summary>
    /// Backwards compatibility.
    /// Returns all OCR text exactly as the old importer did.
    /// </summary>
    public string FullText =>
        string.Join(
            Environment.NewLine + Environment.NewLine,
            Pages.Select(p => p.Text));

    /// <summary>
    /// OCR text from page 1 only.
    /// </summary>
    public string FirstPageText =>
        FirstPage?.Text ?? string.Empty;

    /// <summary>
    /// OCR text from every page except page 1.
    /// </summary>
    public string RemainingPagesText =>
        string.Join(
            Environment.NewLine,
            Pages.Skip(1).Select(p => p.Text));
}