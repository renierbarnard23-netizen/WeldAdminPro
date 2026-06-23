public class WpsRecord
{
    public Guid Id { get; set; }

    public string WpsNumber { get; set; } = string.Empty;
    public int Revision { get; set; }

    // 🔗 LINK TO PQR
    public Guid PqrId { get; set; }
    public string? PqrNumber { get; set; }

    // Welding variables
    public decimal ThicknessMin { get; set; }
    public decimal ThicknessMax { get; set; }

    public decimal? DiameterMin { get; set; }
    public decimal? DiameterMax { get; set; }

    public string? PositionRange { get; set; }

    public int? PNumber { get; set; }
    public int? FNumber { get; set; }

    public bool IsApproved { get; set; }
    public bool IsLocked { get; set; }
    public string ValidationStatus { get; set; } = "";
    public bool IsCompliant { get; set; }
}