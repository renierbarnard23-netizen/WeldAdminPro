namespace WeldAdminPro.Core.Quality.Models
{
    public class TurnoverReadinessRecord
    {
        public string WeldNumber { get; set; }
            = "";

        public bool Released { get; set; }

        public bool NdtAccepted { get; set; }

        public bool NoOpenRepairs { get; set; }

        public bool NoOpenNcrs { get; set; }

        public bool HoldPointsApproved { get; set; }

        public bool DocumentsAttached { get; set; }

        public bool TurnoverReady { get; set; }

        public string BlockingReasons { get; set; }
            = "";
    }
}