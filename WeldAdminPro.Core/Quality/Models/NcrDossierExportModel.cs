using System;
using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models;

public class NcrDossierExportModel
{
    public NcrRecord Ncr { get; set; } =
        new();

    public List<NcrWorkflowHistoryEntry> History { get; set; } =
        new();

    public DateTime GeneratedDate { get; set; } =
        DateTime.Now;

    public string GeneratedBy { get; set; } =
        string.Empty;

    public bool IsFinalDossier =>
        Ncr.IsClosed;
}