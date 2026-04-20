using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;
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
            Load();
        }

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
                    var validator = new WpsValidationService();
                    var (isValid, msg) = validator.Validate(w, pqr);

                    if (!isValid)
                    {
                        status = "INVALID";
                        icon = "❌";
                        color = Brushes.Red;
                        message = msg;
                        InvalidCount++;
                    }
                    else
                    {
                        status = "VALID";
                        icon = "✔";
                        color = Brushes.Green;
                        message = "WPS compliant";
                        ValidCount++;
                    }
                }

                var item = new WpsDisplay
                {
                    Id = w.Id,
                    WpsNumber = w.WpsNumber,
                    Process = w.Process,
                    MaterialGroup = w.MaterialGroup,

                    PNumber = w.PNumber ?? "",
                    FNumber = w.FNumber ?? "",
                    Position = w.Position ?? "",
                    JointType = w.JointType ?? "",
                    Diameter = w.Diameter >= double.MaxValue / 2 ? 0 : w.Diameter,
                    DiameterDisplay = w.Diameter >= double.MaxValue / 2 ? "No Limit" : w.Diameter.ToString("0.##"),

                    ThicknessMin = w.ThicknessMin,
                    ThicknessMax = w.ThicknessMax,
                    PqrNumber = pqr?.PqrNumber ?? "Not Linked",
                    StatusIcon = icon,
                    StatusColor = color,
                    StatusMessage = message,
                    ValidationStatus = status
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
                var wps = _repo.GetAll().First(x => x.Id == SelectedWps.Id);

                wps.WpsNumber = vm.Result.WpsNumber;
                wps.Process = vm.Result.Process;
                wps.MaterialGroup = vm.Result.MaterialGroup;
                wps.PNumber = vm.Result.PNumber;

                wps.ThicknessMin = vm.Result.ThicknessMin;
                wps.ThicknessMax = vm.Result.ThicknessMax;
                wps.Diameter = vm.Result.Diameter;

                wps.Position = vm.Result.Position;
                wps.JointType = vm.Result.JointType;
                wps.FNumber = vm.Result.FNumber;

                var wpsService = new WpsService(_repo);
                wpsService.SaveWps(wps);
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
                    // ✅ Auto-link PQR if found
                    if (!string.IsNullOrWhiteSpace(wps.LinkedPqrNumber))
                    {
                        var pqr = pqrRepo.GetByNumber(wps.LinkedPqrNumber);
                        if (pqr != null)
                            wps.PqrId = pqr.Id;
                    }

                    var wpsService = new WpsService(_repo);
                    wpsService.SaveWps(wps);
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
        private void FilterWarning()
        {
            CurrentFilter = "WARNING";
            Load();
        }
    }
}