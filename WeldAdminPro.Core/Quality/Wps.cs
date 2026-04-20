namespace WeldAdminPro.Core.Quality
{
    public class Wps
    {
        public Guid Id { get; set; }

        public string WpsNumber { get; set; } = "";
        public string Process { get; set; } = "";
        public string MaterialGroup { get; set; } = "";
        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }            
        public bool BackingRequired { get; set; }            // 1G, 2G, 6G
        public string FillerMaterial { get; set; } = string.Empty;
        public string GasType { get; set; } = string.Empty;
        public double AmpsMin { get; set; }
        public double AmpsMax { get; set; }

        public double VoltsMin { get; set; }
        public double VoltsMax { get; set; }

        public double HeatInputMin { get; set; }
        public double HeatInputMax { get; set; }

        public double PreheatMin { get; set; }
        public double PreheatMax { get; set; }
        public double? InterpassMax { get; set; }
        public bool PwhtRequired { get; set; }
        public Guid? PqrId { get; set; }

        public string? PNumber { get; set; }
        public string? FNumber { get; set; }
        public string? Position { get; set; }
        public string? JointType { get; set; }  // Groove / Fillet
        public double Diameter { get; set; }          // Pipe OD
        public string? LinkedPqrNumber { get; set; }

    }
}