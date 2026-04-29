using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class WpsEditorViewModel : ObservableObject
    {
        private readonly WpsRepository _wpsRepo = new();
        private readonly PqrRepository _pqrRepo = new();
        private readonly WpsValidationService _validator = new();
        private readonly WpsComplianceService _compliance = new();

        // =========================
        // DATA
        // =========================

        public ObservableCollection<Pqr> AvailablePqrs { get; } = new();

        [ObservableProperty] private Pqr? selectedPqr;

        // =========================
        // INPUT FIELDS
        // =========================

        [ObservableProperty] private string wpsNumber = "";
        [ObservableProperty] private string process = "";
        [ObservableProperty] private string pNumber = "";
        [ObservableProperty] private string fNumber = "";
        [ObservableProperty] private string position = "";

        [ObservableProperty] private double thicknessMin;
        [ObservableProperty] private double thicknessMax;
        [ObservableProperty] private double diameter;

        [ObservableProperty] private string jointType = "";
        [ObservableProperty] private string jointDesign = "";

        // =========================
        // SETTERS
        // =========================

        partial void OnThicknessMinChanged(double value) => RunCompliance();
        partial void OnThicknessMaxChanged(double value) => RunCompliance();
        partial void OnPositionChanged(string value) => RunCompliance();

        // =========================
        // VALIDATION OUTPUT
        // =========================

        public ObservableCollection<string> ValidationMessages { get; } = new();

        [ObservableProperty] private bool isCompliant;
        [ObservableProperty] private string validationStatus = "";

        public Wps? Result { get; private set; }
        public Action? CloseAction { get; set; }

        // =========================
        // CONSTRUCTOR
        // =========================

        public WpsEditorViewModel()
        {
            LoadPqrs();
        }

        private void LoadPqrs()
        {
            AvailablePqrs.Clear();

            foreach (var p in _pqrRepo.GetAll())
                AvailablePqrs.Add(p);
        }

        // =========================
        // AUTO FILL FROM PQR
        // =========================

        partial void OnSelectedPqrChanged(Pqr? value)
        {
            if (value == null) return;

            Process = value.Process;
            PNumber = value.PNumber;
            FNumber = value.FNumber;
            Position = value.QualifiedPosition;

            ThicknessMin = value.ThicknessQualifiedMin;
            ThicknessMax = value.ThicknessQualifiedMax;

            JointType = value.JointType;
            JointDesign = value.JointDesign;
        }

        // =========================
        // VALIDATION ENGINE
        // =========================

        private void RunCompliance()
        {
            ValidationMessages.Clear();

            if (SelectedPqr == null)
            {
                ValidationMessages.Add("No PQR selected");
                IsCompliant = false;
                return;
            }

            var wps = new Wps
            {
                Process = Process,
                PNumber = PNumber,
                FNumber = FNumber,
                Position = Position,
                ThicknessMin = ThicknessMin,
                ThicknessMax = ThicknessMax,
                Diameter = Diameter,
                JointType = JointType,
                JointDesign = JointDesign
            };

            var result = _compliance.Evaluate(wps, SelectedPqr);

            ValidationMessages.Clear();

            foreach (var msg in result.AllIssues)
                ValidationMessages.Add(msg);

            IsCompliant = result.IsCompliant;
            ValidationStatus = result.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT";

            var errors = _validator.Validate(wps, SelectedPqr);

            foreach (var e in errors)
                ValidationMessages.Add(e);

            if (errors.Any())
            {
                IsCompliant = false;
                ValidationStatus = "NON-COMPLIANT";
            }
            else
            {
                IsCompliant = true;
                ValidationStatus = "COMPLIANT";
            }
        }

        // =========================
        // SAVE
        // =========================

        [RelayCommand]
        private void Save()
        {
            RunCompliance();

            if (!IsCompliant)
            {
                MessageBox.Show("WPS is NOT compliant with selected PQR.");
                return;
            }

            Result = new Wps
            {
                Id = Guid.NewGuid(),
                WpsNumber = WpsNumber,
                PqrId = SelectedPqr?.Id,

                Process = Process,
                PNumber = PNumber,
                FNumber = FNumber,
                Position = Position,

                ThicknessMin = ThicknessMin,
                ThicknessMax = ThicknessMax,
                Diameter = Diameter,

                JointType = JointType,
                JointDesign = JointDesign,

                IsCompliant = true,
                ValidationStatus = ValidationStatus
            };

            _wpsRepo.Add(Result);

            CloseAction?.Invoke();
        }
    }
}