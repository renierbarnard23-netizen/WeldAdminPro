namespace WeldAdminPro.Core.Quality
{
    public class BaseMaterial
    {
        public string Material { get; set; } = "";

        public string Specification { get; set; } = "";

        public string Grade { get; set; } = "";

        public string UNS { get; set; } = "";

        public string Category { get; set; } = "";

        public string Description { get; set; } = "";

        public int PNumber { get; set; }

        public int GroupNumber { get; set; }

        public string Display =>
            $"{Material} (P{PNumber} Gr{GroupNumber})";
    }
}