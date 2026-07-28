namespace WeldAdminPro.Web.Models.Import;

public class ImportWarning
{
    public string Field { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsCritical { get; set; }
}