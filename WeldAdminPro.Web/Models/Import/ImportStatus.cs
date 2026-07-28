namespace WeldAdminPro.Web.Models.Import;

public enum ImportStatus
{
    Ready,
    Uploading,
    ExtractingText,
    Parsing,
    Validating,
    Completed,
    Failed
}