namespace WeldAdminPro.Core.Models
{
    public class JointConfiguration
    {
        public string JointType { get; set; } = "";
        public string JointDesign { get; set; } = "";
        public string ImagePath { get; set; } = "";

        public string SurfacePreparation { get; set; } = "";
        public double GrooveAngle { get; set; }
        public double RootFace { get; set; }
        public double RootGap { get; set; }
        public double GrooveRadius { get; set; }
        public double Misalignment { get; set; }

        public string BackGouging { get; set; } = "";
        public string Backing { get; set; } = "";
        public string BackingType { get; set; } = "";
        public string EdgePreparation { get; set; } = "";
        public string DiagramDisplayName { get; set; } = "";
        public string PassImagePath { get; set; } = "";
    }
}