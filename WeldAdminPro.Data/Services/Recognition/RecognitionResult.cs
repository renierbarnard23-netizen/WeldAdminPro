namespace WeldAdminPro.Data.Services.Recognition;

public class RecognitionResult
{
    public string Material { get; set; } = "";
    public string PNumber { get; set; } = "";
    public string FNumber { get; set; } = "";
    public string PqrNumber { get; set; } = "";
    public string WpsNumber { get; set; } = "";
    public string Position { get; set; } = "";
    public string Joint { get; set; } = "";
    public double TestedThickness { get; set; }
    public double MinimumThickness { get; set; }
    public double MaximumThickness { get; set; }
    public string MaterialText { get; set; } = "";
}