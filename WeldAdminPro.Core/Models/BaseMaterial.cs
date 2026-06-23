namespace WeldAdminPro.Core.Models
{
    public class BaseMaterial
    {
        public string Specification { get; set; } = "";

        public string Grade { get; set; } = string.Empty;
        public int PNumber { get; set; }
        public int GroupNumber { get; set; }

        public string Display => $"{Specification} {Grade} (P{PNumber} Gr{GroupNumber})";
    }
}