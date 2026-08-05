using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models;

public class NcrWorkflowHistoryEntry
{
    public Guid Id { get; set; }

    public Guid NcrId { get; set; }

    public NcrStatus? FromStatus { get; set; }

    public NcrStatus ToStatus { get; set; }

    public string Action { get; set; } =
        string.Empty;

    public string PerformedBy { get; set; } =
        string.Empty;

    public DateTime PerformedDate { get; set; }

    public string Details { get; set; } =
        string.Empty;
}
