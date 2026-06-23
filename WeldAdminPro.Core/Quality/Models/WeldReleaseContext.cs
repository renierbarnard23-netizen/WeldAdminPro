namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldReleaseContext
    {
        public bool HasApprovedWps { get; set; }

        public bool HasQualifiedWelder { get; set; }

        public bool HasAcceptedNdt { get; set; }

        public bool HasOpenRepairs { get; set; }

        public bool HasMaterialTraceability { get; set; }

        public bool HasValidConsumables { get; set; }

        public bool HasCalibrationCompliance { get; set; }
    }
}
