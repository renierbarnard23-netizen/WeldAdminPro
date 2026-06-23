using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality
{
    public class Inspection
    {
        public int Id { get; set; }

        public string Type { get; set; } = string.Empty; // VT, RT, UT

        public string Result { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Inspector { get; set; } = string.Empty;
    }
}