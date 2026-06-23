using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.Views.Quality;


namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class WpsViewModel : ObservableObject
    {
        private readonly WpsRepository _repo = new();

        // =========================
        // PROPERTIES (MANUAL = SAFE)
        // =========================

        private ObservableCollection<WpsDisplay> _wpsList = new();
        public ObservableCollection<WpsDisplay> WpsList
        {
            get => _wpsList;
            set => SetProperty(ref _wpsList, value);
        }

        public ObservableCollection<PqrRecord> AvailablePqrs { get; set; } = new();

        [ObservableProperty]
        private PqrRecord? selectedPqr;

        private WpsDisplay? _selectedWps;
        public WpsDisplay? SelectedWps
        {
            get => _selectedWps;
            set => SetProperty(ref _selectedWps, value);
        }

        private int _validCount;
        public int ValidCount
        {
            get => _validCount;
            set => SetProperty(ref _validCount, value);
        }

        private int _warningCount;
        public int WarningCount
        {
            get => _warningCount;
            set => SetProperty(ref _warningCount, value);
        }

        private int _invalidCount;
        public int InvalidCount
        {
            get => _invalidCount;
            set => SetProperty(ref _invalidCount, value);
        }

        private string _currentFilter = "ALL";
        public string CurrentFilter
        {
            get => _currentFilter;
            set => SetProperty(ref _currentFilter, value);
        }

        private string _alertMessage = "";
        public string AlertMessage
        {
            get => _alertMessage;
            set => SetProperty(ref _alertMessage, value);
        }

        private Brush _alertColor = Brushes.Green;
        public Brush AlertColor
        {
            get => _alertColor;
            set => SetProperty(ref _alertColor, value);
        }

        private string _welderAlertMessage = "";
        public string WelderAlertMessage
        {
            get => _welderAlertMessage;
            set => SetProperty(ref _welderAlertMessage, value);
        }

        private Brush _welderAlertColor = Brushes.Green;
        public Brush WelderAlertColor
        {
            get => _welderAlertColor;
            set => SetProperty(ref _welderAlertColor, value);
        }

        public WpsViewModel()
        {
            LoadPqrs();   // 🔥 ADD THIS
            Load();
        }

        public bool CanCreateWps =>
            PermissionService.HasPermission(
                SystemPermission.CreateWps);

        public bool CanEditWps =>
            PermissionService.HasPermission(
                SystemPermission.EditWps);

        public bool CanDeleteWps =>
            PermissionService.HasPermission(
                SystemPermission.DeleteWps);

        public bool CanApproveWps =>
            PermissionService.HasPermission(
                SystemPermission.ApproveWps);

        public void Load()
        {
            WpsList.Clear();

            ValidCount = 0;
            WarningCount = 0;
            InvalidCount = 0;

            var wpsData = _repo.GetAll();
            var pqrs = new PqrRepository().GetAll();

            foreach (var w in wpsData)
            {
                var pqr = pqrs.FirstOrDefault(p => p.Id == w.PqrId);

                string status, icon, message;
                Brush color;

                if (w.PqrId == null)
                {
                    status = "NO PQR";
                    icon = "❌";
                    color = Brushes.Red;
                    message = "No PQR linked";
                    InvalidCount++;
                }
                else if (pqr == null)
                {
                    status = "PQR NOT FOUND";
                    icon = "❌";
                    color = Brushes.Red;
                    message = "Missing PQR";
                    InvalidCount++;
                }
                else
                {
                    var complianceService =
    new WpsComplianceService();

                    var compliance =
                        complianceService.Evaluate(w, pqr);

                    if (!compliance.IsCompliant)
                    {
                        status = "INVALID";
                        icon = "❌";
                        color = Brushes.Red;

                        message = string.Join(
                            "\n",
                            compliance.ValidationErrors
                                .Concat(compliance.EssentialFailures)
                                .Concat(compliance.RangeFailures));

                        InvalidCount++;
                    }
                    else
                    {
                        status = "VALID";
                        icon = "✔";
                        color = Brushes.Green;

                        message = "✔ COMPLIANT (ASME IX)";

                        ValidCount++;
                    }
                }

                var item = new WpsDisplay
                {
                    Id = w.Id,
                    WpsNumber = w.WpsNumber,
                    Revision = w.Revision,

                    Process = w.Process,
                    MaterialGroup = w.MaterialGroup,

                    PNumber = w.PNumber ?? "",
                    FNumber = w.FNumber ?? "",
                    Position = w.Position ?? "",
                    JointType = w.JointType ?? "",
                    Diameter = w.Diameter,

                    ThicknessMin = w.ThicknessMin,
                    ThicknessMax = w.ThicknessMax,
                    PqrNumber = pqr?.PqrNumber ?? "Not Linked",
                    StatusIcon = icon,
                    StatusColor = color,
                    StatusMessage = message,
                    ValidationStatus = status,

                    ValidationMessage = message,
                };

                if (CurrentFilter == "ALL" ||
                    (CurrentFilter == "INVALID" && icon == "❌") ||
                    (CurrentFilter == "VALID" && icon == "✔"))
                {
                    WpsList.Add(item);
                }
            }

            AlertMessage = InvalidCount > 0
                ? $"❌ {InvalidCount} WPS NOT COMPLIANT"
                : "✔ ALL WPS COMPLIANT";

            AlertColor = InvalidCount > 0 ? Brushes.Red : Brushes.Green;

            var (expired, expiringSoon) = new WelderValidationService().Check();

            if (expired > 0)
            {
                WelderAlertMessage = $"❌ {expired} WELDERS EXPIRED";
                WelderAlertColor = Brushes.Red;
            }
            else if (expiringSoon > 0)
            {
                WelderAlertMessage = $"⚠ {expiringSoon} WELDERS EXPIRING SOON";
                WelderAlertColor = Brushes.Orange;
            }
            else
            {
                WelderAlertMessage = "✔ ALL WELDERS VALID";
                WelderAlertColor = Brushes.Green;
            }
        }

        private void LoadPqrs()
        {
            var repo = new PqrRepository();

            var pqrs = repo.GetAll();

            AvailablePqrs = new ObservableCollection<PqrRecord>(
                pqrs.Select(p => new PqrRecord
                {
                    Id = p.Id,
                    PqrNumber = p.PqrNumber
                })
            );
        }


        // =========================
        // COMMANDS
        // =========================

        [RelayCommand] private void Refresh() => Load();
        [RelayCommand] private void FilterAll() { CurrentFilter = "ALL"; Load(); }
        [RelayCommand] private void FilterInvalid() { CurrentFilter = "INVALID"; Load(); }
        [RelayCommand] private void FilterValid() { CurrentFilter = "VALID"; Load(); }

        [RelayCommand]
        private void AddWps()
        {
            var dialog = new WpsDialog();
            var vm = new WpsDialogViewModel();

            dialog.DataContext = vm;
            vm.CloseAction = () => dialog.DialogResult = true;

            if (dialog.ShowDialog() == true && vm.Result != null)
            {
                _repo.Add(vm.Result);
                Load();
            }
        }

        [RelayCommand]
        private void EditWps()
        {

            if (!PermissionService.HasPermission(
    SystemPermission.EditWps))
            {
                MessageBox.Show(
                    "You do not have permission to edit WPS documents.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (SelectedWps == null)
            {
                MessageBox.Show("Select a WPS first");
                return;
            }

            var dialog = new WpsDialog();
            var vm = new WpsDialogViewModel
            {
                WpsNumber = SelectedWps.WpsNumber,
                Process = SelectedWps.Process,
                MaterialGroup = SelectedWps.MaterialGroup,
                PNumber = SelectedWps.PNumber,

                ThicknessMin = SelectedWps.ThicknessMin,
                ThicknessMax = SelectedWps.ThicknessMax,
                Diameter = SelectedWps.Diameter,

                Position = SelectedWps.Position,
                JointType = SelectedWps.JointType,
                FNumber = SelectedWps.FNumber
            };

            dialog.DataContext = vm;
            vm.CloseAction = () => dialog.DialogResult = true;

            if (dialog.ShowDialog() == true && vm.Result != null)
            {
                var original =
    _repo.GetAll()
        .First(x => x.Id == SelectedWps.Id);

                Wps workingCopy;

                // =====================================
                // APPROVED WPS = CREATE REVISION
                // =====================================

                if (original.IsApproved)
                {
                    original.IsActive = false;

                    _repo.Update(original);

                    workingCopy =
                        original.CreateRevision(
                            CurrentUserContext.Username);
                }
                else
                {
                    workingCopy = original;
                }

                // =====================================
                // APPLY CHANGES
                // =====================================

                workingCopy.WpsNumber =
                    vm.Result.WpsNumber;

                workingCopy.Process =
                    vm.Result.Process;

                workingCopy.MaterialGroup =
                    vm.Result.MaterialGroup;

                workingCopy.PNumber =
                    vm.Result.PNumber;

                workingCopy.ThicknessMin =
                    vm.Result.ThicknessMin;

                workingCopy.ThicknessMax =
                    vm.Result.ThicknessMax;

                workingCopy.Diameter =
                    vm.Result.Diameter;

                workingCopy.Position =
                    vm.Result.Position;

                workingCopy.JointType =
                    vm.Result.JointType;

                workingCopy.FNumber =
                    vm.Result.FNumber;

                // =====================================
                // SAVE
                // =====================================

                if (original.IsApproved)
                {
                    _repo.Add(workingCopy);
                }
                else
                {
                    _repo.Update(workingCopy);
                }

                Load();
            }
        }

        [RelayCommand]
        private void DeleteWps()
        {
            if (SelectedWps == null) return;

            _repo.Delete(SelectedWps.Id);
            Load();
        }

        [RelayCommand]
        private void LinkPqr()
        {
            if (SelectedWps == null)
            {
                MessageBox.Show("Select a WPS first");
                return;
            }

            var pqrs = new PqrRepository().GetAll();
            var dialog = new LinkPqrWindow(pqrs);

            if (dialog.ShowDialog() == true && dialog.SelectedPqr != null)
            {
                var wps = _repo.GetAll().First(x => x.Id == SelectedWps.Id);
                wps.PqrId = dialog.SelectedPqr.Id;
                _repo.Update(wps);
                Load();
            }
        }

        [RelayCommand]
        private void ImportPdf()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var pdfService = new PdfToImageService();
                var ocrService = new WpsOcrService();
                var parser = new WpsParserService();
                var pqrRepo = new PqrRepository();
                var wpsData = _repo.GetActive();
                

                // ✅ Extract filename correctly
                var fileName = Path.GetFileName(dialog.FileName);

                // 1. Convert PDF → Image
                var imagePath = pdfService.ConvertFirstPage(dialog.FileName);

                // 2. OCR → Text
                var text = ocrService.ExtractText(imagePath);

                // 3. Parse → MULTIPLE WPS
                var wpsList = parser.ParseMultiple(text, fileName);

                int imported = 0;

                foreach (var wps in wpsList)
                {
                    // ✅ Auto-link PQR if foreach (var w in wpsData)
                    if (!string.IsNullOrWhiteSpace(wps.LinkedPqrNumber))
                    {
                        var pqr = pqrRepo.GetByNumber(wps.LinkedPqrNumber);
                        if (pqr != null)
                            wps.PqrId = pqr.Id;
                    }

                    var wpsService = new WpsService(_repo);
                    wpsService.SaveRevision(wps);
                    imported++;
                }

                // 4. Refresh UI
                Load();

                MessageBox.Show($"{imported} WPS(s) imported successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}");
            }
        }
        [RelayCommand]
        private void Generate()
        {
            if (SelectedPqr == null)
                return;

            OnSelectedPqrChanged(SelectedPqr);
        }

        [RelayCommand]
        private void FilterWarning()
        {
            CurrentFilter = "WARNING";
            Load();
        }

        [RelayCommand]
        private void ApproveWps()
        {
            if (SelectedWps == null)
                return;

            var wps = _repo.GetAll().First(x => x.Id == SelectedWps.Id);

            if (wps.IsLocked)
                return;

            wps.IsApproved = true;
            wps.IsLocked = true;
            wps.ApprovedOn = DateTime.Now;
            wps.ApprovedBy = "User";

            _repo.Update(wps);
            Load();
        }
    }
}