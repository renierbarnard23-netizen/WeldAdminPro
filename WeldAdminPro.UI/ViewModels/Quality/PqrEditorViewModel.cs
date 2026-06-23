using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Services;
using static SkiaSharp.HarfBuzz.SKShaper;
using ValidationResult = WeldAdminPro.Core.Models.ValidationResult;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class PqrEditorViewModel : ObservableObject
    {
        // =========================
        // RESULT + CLOSE
        // =========================

        public Pqr? Result { get; private set; }
        public Action? CloseAction { get; set; }

        private Guid? _editingId;

        // =========================
        // HEADER
        // =========================

        [ObservableProperty] private string pqrNumber = "";
        [ObservableProperty] private string wpsNumber = "";
        [ObservableProperty] private bool isLocked;
        [ObservableProperty] private bool isCompliant;
        [ObservableProperty] private string validationStatus = "";
        [ObservableProperty] private string selectedJointImage = "";


        partial void OnPqrNumberChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(WpsNumber) && !string.IsNullOrWhiteSpace(value))
                WpsNumber = value.Replace("PQR", "WPS");
        }

        // =========================
        // STANDARD + PROCESS
        // =========================

        public ObservableCollection<string> WeldingStandards { get; } = new()
{
    // AWS
    "AWS D1.1 – Structural Steel",
    "AWS D1.2 – Aluminum",
    "AWS D1.5 – Bridge Welding",
    "AWS D1.6 – Stainless Steel",
    "AWS D1.3 – Sheet Steel",
    "AWS D17.1 – Aerospace",
    "AWS A2.4 – Welding Symbols",
    "AWS B2.1 – Procedure Qualification",
    "AWS Z49.1 – Safety in Welding, Cutting, and Allied Processes",

    // ASME
    "ASME BPVC Section IX",
    "ASME B31.1 – Power Piping",
    "ASME B31.3 – Process Piping",

    // API
    "API 1104 – Pipelines",
    "API 650 – Welded Tanks for Oil Storage",
    "API 620 – Low Pressure Storage Tanks",
    "API 12F – Field Welded Tanks",
    "API 2B – Offshore Structures",
    "API 2RD – Repair of Offshore Structures",
    "API 5L – Line Pipe",

    // ISO
    "ISO 3834 – Welding Quality",
    "ISO 9606 – Welder Qualification",
    "ISO 15614 – Procedure Qualification",
    "ISO 4063 – Welding Processes",
    "ISO 5817 – Weld Quality Levels",

    // CSA
    "CSA W47.1",
    "CSA W59",

    // EN
    "BS EN 13445 – Pressure Vessels",
    "BS EN 13480 – Piping",
    "BS EN 1090 – Structural Steel",
    "BS EN 15085 – Railway Applications",
    "BS EN 1011 – Welding of Steel",
    "BS EN 287 – Welder Qualification",
    "BS EN 15614 – Procedure Qualification",
    "BS EN 9606 – Welder Qualification",
    "BS EN 1011 – Welding of Steel",

    // Regional
    "DIN Standards",
    "BS Standards",
    "SANS 347"
};

        public ObservableCollection<string> WeldingProcesses { get; } = new()
        {
            "TIG",
            "MIG",
            "SMAW",
            "FCAW"
        };

        [ObservableProperty] private string selectedStandard = "ASME BPVC Section IX";
        [ObservableProperty] private string selectedProcess = "TIG";

        // =========================
        // JOINTS
        // =========================

        [ObservableProperty] private string selectedJointDiagram = "";
        [ObservableProperty] private string selectedPassDiagram = "";

        public ObservableCollection<JointConfiguration> JointLibrary { get; } = new()
{
    // =========================
    // GROOVE JOINTS
    // =========================
    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Single V",
        ImagePath = "/Images/Joints/single_v.png",
        PassImagePath = "/Images/Passes/single_v_pass.png",
        SurfacePreparation = "Machining",
        GrooveAngle = 60,
        RootFace = 2,
        RootGap = 2,
        EdgePreparation = "Machined"
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Double V",
        ImagePath = "/Images/Joints/double_v.png",
        PassImagePath = "/Images/Passes/double_v_pass.png",
        GrooveAngle = 60,
        RootFace = 2,
        RootGap = 2
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Single U",
        ImagePath = "/Images/Joints/single_u.png",
        PassImagePath = "/Images/Passes/single_u_pass.png",
        GrooveRadius = 6,
        RootGap = 2
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Double U",
        ImagePath = "/Images/Joints/double_u.png",
        PassImagePath = "/Images/Passes/double_u_pass.png",
        GrooveRadius = 6,
        RootGap = 2
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Single Bevel",
        ImagePath = "/Images/Joints/single_bevel.png",
        PassImagePath = "/Images/Passes/single_bevel_pass.png",
        GrooveAngle = 45,
        RootGap = 2
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Double Bevel",
        ImagePath = "/Images/Joints/double_bevel.png",
        PassImagePath = "/Images/Passes/double_bevel_pass.png",
        GrooveAngle = 45,
        RootGap = 2
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Single J",
        ImagePath = "/Images/Joints/single_j.png",
        PassImagePath = "/Images/Passes/single_j_pass.png"
    },

    new JointConfiguration
    {
        JointType = "Groove",
        JointDesign = "Double J",
        ImagePath = "/Images/Joints/double_j.png",
        PassImagePath = "/Images/Passes/double_j_pass.png"
    },

    // =========================
    // FILLET JOINTS
    // =========================
    new JointConfiguration
    {
        JointType = "Fillet",
        JointDesign = "T-Joint",
        ImagePath = "/Images/Joints/t_joint.png",
        PassImagePath = "/Images/Passes/t_joint_pass.png"
    },

    new JointConfiguration
    {
        JointType = "Fillet",
        JointDesign = "Lap Joint",
        ImagePath = "/Images/Joints/lap_joint.png",
        PassImagePath = "/Images/Passes/lap_joint_pass.png"
    },

    new JointConfiguration
    {
        JointType = "Fillet",
        JointDesign = "Corner Joint",
        ImagePath = "/Images/Joints/corner.png",
        PassImagePath = "/Images/Passes/corner_pass.png"
    }
};

        public ObservableCollection<string> JointTypes { get; } = new()
        {
            "Groove",
            "Fillet",
            "Plug",
            "Slot",
            "Spot",
            "Seam",
            "Surfacing"
        };
        public ObservableCollection<string> FilteredJointDesigns { get; } = new();

        [ObservableProperty] private string selectedJointType = "Groove";
        [ObservableProperty] private string selectedJointDesign = "";
        
        [ObservableProperty] private string surfacePreparation = "";
        [ObservableProperty] private double grooveAngle;
        [ObservableProperty] private double rootFace;
        [ObservableProperty] private double rootGap;

        // =========================
        // JOINT VARIABLES
        // =========================

        public ObservableCollection<string> SurfacePreparations { get; } = new()
        {
            "Grinding", "Machining", "Chemical Cleaning"
        };

        [ObservableProperty] private double grooveRadius;
        [ObservableProperty] private double misalignment;

        public ObservableCollection<string> BackGougingOptions { get; } = new()
        {
            "None", "Grinding", "Air Carbon Arc"
        };

        [ObservableProperty] private string backGouging = "None";

        public ObservableCollection<string> BackingOptions { get; } = new()
        {
            "None",
            "Ceramic",
            "Metal",
            "Flux",
            "Gas"
        };

        [ObservableProperty] private string backing = "None";
        [ObservableProperty] private string backingType = "";
        [ObservableProperty] private string edgePreparation = "";

        public ObservableCollection<string> EdgePreparations { get; } = new()
        {
            "Machining",
            "Grinding",
            "Thermal Cutting",
            "Plasma Cutting",
            "Oxy-Fuel Cutting"
        };

        [ObservableProperty] private string pqrRevision = "0";
        [ObservableProperty] private DateTime? pqrDate = DateTime.Now;

        [ObservableProperty] private string wpsRevision = "0";
        [ObservableProperty] private DateTime? wpsDate = DateTime.Now;

        // =========================
        // MATERIALS
        // =========================

        public ObservableCollection<BaseMaterial> BaseMaterialLibrary { get; } = new()
{
    // P1 - Carbon Steel
    new BaseMaterial { Specification="SA-36", Grade="", PNumber=1, GroupNumber=1 },
    new BaseMaterial { Specification="SA-106", Grade="B", PNumber=1, GroupNumber=1 },
    new BaseMaterial { Specification="SA-516", Grade="70", PNumber=1, GroupNumber=1 },

    // P3
    new BaseMaterial { Specification="SA-213", Grade="T2", PNumber=3, GroupNumber=1 },

    // P4
    new BaseMaterial { Specification="SA-335", Grade="P11", PNumber=4, GroupNumber=1 },

    // P5
    new BaseMaterial { Specification="SA-335", Grade="P22", PNumber=5, GroupNumber=1 },

    // P8 - Stainless
    new BaseMaterial { Specification="SA-240", Grade="304L", PNumber=8, GroupNumber=1 },
    new BaseMaterial { Specification="SA-240", Grade="316L", PNumber=8, GroupNumber=1 },

    // Duplex / Super Duplex
    new BaseMaterial { Specification="SAF2205", Grade="", PNumber=10, GroupNumber=1 },
    new BaseMaterial { Specification="SAF2507", Grade="", PNumber=10, GroupNumber=1 },

    // Aluminium
    new BaseMaterial { Specification="AA6061", Grade="", PNumber=21, GroupNumber=1 },

    // Nickel
    new BaseMaterial { Specification="Inconel 625", Grade="", PNumber=43, GroupNumber=1 }
};

        [ObservableProperty] private BaseMaterial? selectedBaseMaterial1;

        [ObservableProperty] private BaseMaterial? selectedBaseMaterial2;

        [ObservableProperty] private string baseMetal2PNo = "";
        [ObservableProperty] private string baseMetal2Group = "";

        partial void OnSelectedBaseMaterial2Changed(BaseMaterial? value)
        {
            BaseMetal2PNo = value?.PNumber.ToString() ?? "";
            BaseMetal2Group = value?.GroupNumber.ToString() ?? "";
        }


        [ObservableProperty]
        private string baseMetal1PNo = "";

        [ObservableProperty]
        private string baseMetal1Group = "";
        
        public ObservableCollection<FillerMaterial> FillerLibrary { get; } = new()
        {
        // =========================
        // SMAW (Stick)
        // =========================
            new FillerMaterial { Classification = "E6010", FNumber = 3, ANumber = 1 },
            new FillerMaterial { Classification = "E6013", FNumber = 2, ANumber = 1 },
            new FillerMaterial { Classification = "E7018", FNumber = 4, ANumber = 1 },

        // =========================
        // GMAW / GTAW (Solid Wire)
        // =========================
            new FillerMaterial { Classification = "ER70S-6", FNumber = 6, ANumber = 1 },
            new FillerMaterial { Classification = "ER308L", FNumber = 6, ANumber = 8 },
            new FillerMaterial { Classification = "ER316L", FNumber = 6, ANumber = 8 },

        // =========================
        // ALLOYS (Nickel etc.)
        // =========================
            new FillerMaterial { Classification = "ERNiCr-3", FNumber = 43, ANumber = 0 },
            new FillerMaterial { Classification = "ERNiMo-3", FNumber = 45, ANumber = 0 }
        };

        [ObservableProperty] private FillerMaterial? selectedFiller;

        public string FNumber => SelectedFiller?.FNumber.ToString() ?? "";
        public string ANumber => SelectedFiller?.ANumber.ToString() ?? "";

        public class FillerPass
        {
            public string PassName { get; set; } = "";
            public double Size { get; set; }
        }

        public ObservableCollection<FillerPass> FillerPasses { get; } = new()
{
    new FillerPass { PassName = "Pass 1" },
    new FillerPass { PassName = "Pass 2" },
    new FillerPass { PassName = "Pass 3" }
};

        [ObservableProperty] private string? sfaNumber;
        [ObservableProperty] private string? awsClassification;

        partial void OnSelectedFillerChanged(FillerMaterial? value)
        {
            OnPropertyChanged(nameof(FNumber));
            OnPropertyChanged(nameof(ANumber));
        }

        // =========================
        // QUALIFICATION INPUTS
        // =========================

        public ObservableCollection<string> ProductTypes { get; } = new() { "Plate", "Pipe" };
        public ObservableCollection<string> WeldingPositions { get; } = new() { "1G", "2G", "3G", "4G", "5G", "6G" };

        [ObservableProperty] private string selectedProductType = "Plate";
        [ObservableProperty] private string selectedPosition = "1G";

        [ObservableProperty] private double testThickness;
        [ObservableProperty] private double testDiameter;

        [ObservableProperty] private string polarity = "DCEN";
        [ObservableProperty] private double travelSpeed;
        [ObservableProperty] private double heatInput;

        [ObservableProperty] private string weldingType = "Manual";

        public ObservableCollection<string> WeldingTypes { get; } = new()
        {
            "Manual",
            "Semi-Automatic",
            "Automatic"
        };

        public ObservableCollection<string> ProgressionTypes { get; } = new()
        {
            "Up",
            "Down"
        };

        
        [ObservableProperty] private string qualifiedPNumberRange = "";

        private void CalculateMaterialQualification()
        {
            if (string.IsNullOrWhiteSpace(BaseMetal1PNo))
            {
                QualifiedPNumberRange = "Not defined";
                return;
            }

            // Simplified ASME logic
            if (BaseMetal1PNo == "1")
                QualifiedPNumberRange = "P-No 1 only";
            else if (BaseMetal1PNo == "8")
                QualifiedPNumberRange = "P-No 8 only (Stainless)";
            else
                QualifiedPNumberRange = $"P-No {BaseMetal1PNo}";
        }

        [ObservableProperty] private string selectedProgression = "Up";

        // =========================
        // OUTPUTS
        // =========================

        [ObservableProperty] private double qualifiedMinThickness;
        [ObservableProperty] private double qualifiedMaxThickness;
        [ObservableProperty] private string qualifiedPositionRange = "";

        // =========================
        // VALIDATION
        // =========================

        public ObservableCollection<Core.Models.ValidationResult> ValidationMessages { get; } = new();


        // =========================
        // EVENTS
        // =========================

        partial void OnSelectedJointTypeChanged(string value)
        {
            FilteredJointDesigns.Clear();

            foreach (var d in JointLibrary.Where(j => j.JointType == value))
                FilteredJointDesigns.Add(d.JointDesign);

            SelectedJointDesign = FilteredJointDesigns.FirstOrDefault() ?? "";
        }

        partial void OnSelectedJointDesignChanged(string value)
        {
            var joint = JointLibrary.FirstOrDefault(j =>
                j.JointDesign == value &&
                j.JointType == SelectedJointType);

            if (joint == null) return;

            SurfacePreparation = joint.SurfacePreparation;
            GrooveAngle = joint.GrooveAngle;
            RootFace = joint.RootFace;
            RootGap = joint.RootGap;

            GrooveRadius = joint.GrooveRadius;
            Misalignment = joint.Misalignment;
            BackingType = joint.BackingType;
            EdgePreparation = joint.EdgePreparation;

            // ✅ FIXED LINE (THIS IS WHAT YOU WERE ASKING)
            SelectedJointImage = $"pack://application:,,,/{joint.ImagePath.TrimStart('/')}";

            SelectedPassDiagram = string.IsNullOrWhiteSpace(joint.PassImagePath)
                ? "pack://application:,,,/Images/Passes/default.png"
                : $"pack://application:,,,/{joint.PassImagePath.TrimStart('/')}";

            CalculateQualification();
        }

        private void CalculateQualification()
        {
            var pqr = new Pqr
            {
                ThicknessTested = TestThickness,
                DiameterMax = TestDiameter,
                QualifiedPosition = SelectedPosition,
                IsPipe = SelectedProductType == "Pipe"
            };

            var result = _rangeEngine.Calculate(pqr);

            QualifiedMinThickness = result.MinThickness;
            QualifiedMaxThickness = result.MaxThickness;
            QualifiedPositionRange = result.QualifiedPosition;

            // 🔥 ADD THIS
            QualifiedDiameterRange = result.MaxDiameter == double.MaxValue
                ? "Unlimited"
                : $"{result.MinDiameter} – {result.MaxDiameter}";
        }

        partial void OnSelectedBaseMaterial1Changed(BaseMaterial? value)
        {
            BaseMetal1PNo = value?.PNumber.ToString() ?? "";
            BaseMetal1Group = value?.GroupNumber.ToString() ?? "";

            CalculateMaterialQualification();   // ✅ ADD THIS
            CalculateQualification();
        }

        // =========================
        // ENGINE
        // =========================
        [ObservableProperty] private string qualifiedDiameterRange = "";

        private readonly QualificationRangeEngine _rangeEngine = new();               

        private bool HasErrors;

        private void RunValidation()
        {
            ValidationMessages.Clear();
            HasErrors = false;

            void Add(string msg, string severity)
            {
                ValidationMessages.Add(new ValidationResult(msg, severity));
                if (severity == "Error") HasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(PqrNumber))
                Add("PQR Number is required", "Error");

            if (string.IsNullOrWhiteSpace(SelectedStandard))
                Add("Standard must be selected", "Error");

            if (string.IsNullOrWhiteSpace(BaseMetal1PNo))
                Add("Base metal P-No required", "Error");

            if (TestThickness <= 0)
                Add("Test thickness must be > 0", "Error");

            if (string.IsNullOrWhiteSpace(FNumber))
                Add("F-No missing", "Error");

            if (SelectedPosition == "6G" && SelectedProductType != "Pipe")
                Add("6G only valid for pipe", "Error");

            if (GrooveAngle == 0)
                Add("Groove angle not specified", "Warning");
        }

        // =========================
        // SAVE
        // =========================

        [RelayCommand]
        private void Save()
        {
            RunValidation();
            CalculateQualification();

            var qualification =
    _rangeEngine.Calculate(
        new Pqr
        {
            ThicknessTested = TestThickness,
            DiameterMax = TestDiameter,
            QualifiedPosition = SelectedPosition,
            IsPipe = SelectedProductType == "Pipe"
        });

            if (HasErrors)
            {
                IsCompliant = false;
                ValidationStatus = "Errors detected";   // 🔥 ADD THIS

                MessageBox.Show("Fix validation errors before saving.");
                return;
            }

            IsCompliant = true;
            ValidationStatus = "Valid";

            // ✅ NOW create final Result
            Result = new Pqr
            {
                Id = _editingId ?? Guid.NewGuid(),
                PqrNumber = PqrNumber,
                WpsReferenceNumber = WpsNumber,
                QualificationDate = DateTime.Now,
                QualifiedBy = Environment.UserName,

                Process = SelectedProcess,
                Standard = SelectedStandard,

                JointType = SelectedJointType,
                JointDesign = SelectedJointDesign,

                SurfacePreparation = SurfacePreparation,
                GrooveAngle = GrooveAngle,
                RootFace = RootFace,
                RootGap = RootGap,
                Backing = Backing,
                BackGouging = BackGouging,

                PNumber = BaseMetal1PNo,
                FNumber = FNumber,

                ThicknessQualifiedMin = qualification.MinThickness,

                ThicknessQualifiedMax = qualification.MaxThickness,

                QualifiedPosition = qualification.QualifiedPosition,

                DiameterMin = qualification.MinDiameter,

                DiameterMax = qualification.MaxDiameter,

                IsPipe = SelectedProductType == "Pipe",
                    QualifiedPNumberRange = QualifiedPNumberRange,

                MaterialGroup = string.IsNullOrWhiteSpace(BaseMetal2PNo)
                    ? $"P{BaseMetal1PNo} Gr {BaseMetal1Group}"
                    : $"P{BaseMetal1PNo} to P{BaseMetal2PNo}",
                    Position = SelectedPosition,

                PassDiagramPath = SelectedPassDiagram,

                GrooveRadius = GrooveRadius,
                Misalignment = Misalignment,
                BackingType = BackingType,
                EdgePreparation = EdgePreparation,

                WeldingType = WeldingType,
                Progression = SelectedProgression,

                PNumber2 = BaseMetal2PNo,

                Polarity = Polarity,
                TravelSpeed = TravelSpeed,
                HeatInput = HeatInput
            };

            CloseAction?.Invoke();
        }

        // =========================
        // APPROVE (FIXED)
        // =========================

        [RelayCommand]
        private void Approve()
        {
            Save();

            if (Result == null)
                return;

            Result.IsApproved = true;
            Result.IsLocked = true;
            Result.ApprovedOn = DateTime.Now;
            Result.ApprovedBy = Environment.UserName;
        }

        // =========================
        // public ObservableCollection<FillerMaterial> FillerLibrary
        // =========================
                
        // =========================
        // CONSTRUCTORS
        // =========================
      

        public PqrEditorViewModel()
        {
            OnSelectedJointTypeChanged(SelectedJointType);
            CalculateQualification();
        }

        public PqrEditorViewModel(Pqr existing) : this()
        {
            _editingId = existing.Id;

            PqrNumber = existing.PqrNumber;
            WpsNumber = existing.WpsReferenceNumber;

            SelectedProcess = existing.Process;
            SelectedJointType = existing.JointType;

            TestThickness = existing.ThicknessTested;

            // 🔒 LOCK STATE
            IsLocked = existing.IsLocked;

            if (IsLocked)
            {
                MessageBox.Show("This PQR is locked and cannot be modified.");
                return;
            }
        }
    }
}