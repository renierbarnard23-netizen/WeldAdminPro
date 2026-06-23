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
        private readonly WpsComplianceService _compliance = new();

        // =========================
        // COMPLIANCE RESULT (SINGLE SOURCE OF TRUTH)
        // =========================

        private WpsComplianceResult _complianceResult = new();
        public WpsComplianceResult ComplianceResult
        {
            get => _complianceResult;
            set
            {
                _complianceResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCompliant));
                OnPropertyChanged(nameof(ValidationStatus));
            }
        }

        public bool IsCompliant => ComplianceResult?.IsCompliant == true;

        public string ValidationStatus => IsCompliant ? "COMPLIANT" : "NOT COMPLIANT";

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

            RunCompliance();
        }

        // =========================
        // LIVE COMPLIANCE
        // =========================

        partial void OnThicknessMinChanged(double value) => RunCompliance();
        partial void OnThicknessMaxChanged(double value) => RunCompliance();
        partial void OnPositionChanged(string value) => RunCompliance();
        partial void OnDiameterChanged(double value) => RunCompliance();

        private void RunCompliance()
        {
            if (SelectedPqr == null)
            {
                ComplianceResult = new WpsComplianceResult();
                ComplianceResult.ValidationErrors.Add("No PQR selected");
                return;
            }

            var wps = new Wps
            {
                WpsNumber = WpsNumber,
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

            ComplianceResult = _compliance.Evaluate(wps, SelectedPqr);
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