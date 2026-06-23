using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality
{
    public class Material
    {
        public int Id { get; set; }

        public string MaterialGrade { get; set; } = string.Empty;

        public string HeatNumber { get; set; } = string.Empty;

        public string CertificateNumber { get; set; } = string.Empty;
    }
}