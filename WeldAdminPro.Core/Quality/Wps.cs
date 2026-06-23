namespace WeldAdminPro.Core.Quality
{
    public class Wps
    {
        public Guid Id { get; set; }

        public string WpsNumber { get; set; } = "";
        public string Process { get; set; } = "";

        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }

        public double Diameter { get; set; }

        public string FillerMaterial { get; set; } = "";
        public string GasType { get; set; } = "";

        public double AmpsMin { get; set; }
        public double AmpsMax { get; set; }

        public double VoltsMin { get; set; }
        public double VoltsMax { get; set; }

        public double HeatInputMin { get; set; }
        public double HeatInputMax { get; set; }

        public double PreheatMin { get; set; }
        public double PreheatMax { get; set; }

        public string WelderNumber { get; set; } = "";
        public double? InterpassMax { get; set; }
        public bool PwhtRequired { get; set; }
        private (double min, double max) ExtractTestThickness(string text)
        {
            var parts = text.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                double.TryParse(parts[0].Trim(), out double min) &&
                double.TryParse(parts[1].Trim(), out double max))
            {
                return (min, max);
            }
            throw new FormatException("Invalid thickness range format. Expected format: 'min - max'.");
        }

        public Guid? PqrId { get; set; }
        public string? LinkedPqrNumber { get; set; }

        public string? PNumber { get; set; }
        public string? FNumber { get; set; }
        public string? Position { get; set; }
        public string MaterialGroup { get; set; } = "";

        public string? JointType { get; set; }
        public string JointDesign { get; set; } = "";

        public string Backing { get; set; } = "";
        public string BackingType { get; set; } = "";

        public bool IsApproved { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedBy { get; set; }

        public double ParseConfidence
        {
            get;
            set;
        }

        public string ParseNotes
        {
            get;
            set;
        } = "";

        public Guid? PreviousRevisionId
        {
            get;
            set;
        }

        public DateTime CreatedOn
        {
            get;
            set;
        } = DateTime.UtcNow;

        public string CreatedBy
        {
            get;
            set;
        } = "";

        public string ImportSource
        {
            get;
            set;
        } = "";

        public int Revision { get; set; }
        public bool IsActive { get; set; }

        // 🔥 Compliance
        public bool IsCompliant { get; set; }
        public string ValidationStatus { get; set; } = "";

        // 🔥 Essential Variables
        public string CurrentType { get; set; } = "";
        public string Polarity { get; set; } = "";
        public string Progression { get; set; } = "";

        // 🔥 Visual Traceability
        public string JointDiagramPath { get; set; } = "";
        public string PassDiagramPath { get; set; } = "";

        // 🔥 Audit
        public string QualifiedThicknessRange { get; set; } = "";


        public Wps CreateRevision(
        string username)
        {
            return new Wps
            {
                Id = Guid.NewGuid(),

                PreviousRevisionId =
                    Id,

                Revision =
                    Revision + 1,

                IsApproved =
                    false,

                IsLocked =
                    false,

                ApprovedBy =
                    null,

                ApprovedOn =
                    null,

                IsActive =
                    true,

                CreatedOn =
                    DateTime.UtcNow,

                CreatedBy =
                    username,

                ImportSource =
                    ImportSource,

                // =====================================
                // COPY ENGINEERING DATA
                // =====================================

                WpsNumber = WpsNumber,
                Process = Process,
                ThicknessMin = ThicknessMin,
                ThicknessMax = ThicknessMax,
                Diameter = Diameter,
                FillerMaterial = FillerMaterial,
                GasType = GasType,
                AmpsMin = AmpsMin,
                AmpsMax = AmpsMax,
                VoltsMin = VoltsMin,
                VoltsMax = VoltsMax,
                HeatInputMin = HeatInputMin,
                HeatInputMax = HeatInputMax,
                PreheatMin = PreheatMin,
                PreheatMax = PreheatMax,
                InterpassMax = InterpassMax,
                PwhtRequired = PwhtRequired,
                PqrId = PqrId,
                LinkedPqrNumber = LinkedPqrNumber,
                PNumber = PNumber,
                FNumber = FNumber,
                Position = Position,
                MaterialGroup = MaterialGroup,
                JointType = JointType,
                JointDesign = JointDesign,
                Backing = Backing,
                BackingType = BackingType,
                IsCompliant = IsCompliant,
                ValidationStatus = ValidationStatus,
                CurrentType = CurrentType,
                Polarity = Polarity,
                Progression = Progression,
                JointDiagramPath = JointDiagramPath,
                PassDiagramPath = PassDiagramPath,
                QualifiedThicknessRange =
                    QualifiedThicknessRange,

                ParseConfidence =
                    ParseConfidence,

                ParseNotes =
                    ParseNotes
            };
        }
    }
}