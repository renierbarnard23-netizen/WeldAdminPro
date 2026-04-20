using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.Views.Quality;
using System.Runtime.Versioning;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class PqrViewModel : ObservableObject
    {
        private readonly PqrRepository _repo = new();

        [ObservableProperty]
        private ObservableCollection<Pqr> pqrList = new();

        [ObservableProperty]
        private Pqr? selectedPqr;

        public PqrViewModel()
        {
            Load();
        }

        public void Load()
        {
            PqrList.Clear();

            var data = _repo.GetAll();

            foreach (var p in data)
                PqrList.Add(p);
        }

        [RelayCommand]
        private void Refresh()
        {
            Load();
        }

        // =========================
        // ADD
        // =========================
        [RelayCommand]
        private void AddPqr()
        {
            var dialog = new PqrDialog();
            var vm = new PqrDialogViewModel();

            dialog.DataContext = vm;
            vm.CloseAction = () => dialog.DialogResult = true;

            if (dialog.ShowDialog() == true && vm.Result != null)
            {
                _repo.Add(vm.Result);
                Load();
            }
        }

        // =========================
        // EDIT
        // =========================
        [RelayCommand]
        private void EditPqr()
        {
            if (SelectedPqr == null)
            {
                MessageBox.Show("Select a PQR first");
                return;
            }

            var dialog = new PqrDialog();
            var vm = new PqrDialogViewModel
            {
                PqrNumber = SelectedPqr.PqrNumber,
                Process = SelectedPqr.Process,
                MaterialGroup = SelectedPqr.MaterialGroup,
                PNumber = SelectedPqr.PNumber,

                ThicknessQualifiedMin = SelectedPqr.ThicknessQualifiedMin,
                ThicknessQualifiedMax = SelectedPqr.ThicknessQualifiedMax,

                FNumber = SelectedPqr.FNumber,
                QualifiedPosition = SelectedPqr.QualifiedPosition,
                JointType = SelectedPqr.JointType,

                DiameterMin = SelectedPqr.DiameterMin,
                DiameterMax = SelectedPqr.DiameterMax
            };

            dialog.DataContext = vm;
            vm.CloseAction = () => dialog.DialogResult = true;

            if (dialog.ShowDialog() == true && vm.Result != null)
            {
                var pqr = _repo.GetAll().First(x => x.Id == SelectedPqr.Id);

                pqr.PqrNumber = vm.Result.PqrNumber;
                pqr.Process = vm.Result.Process;
                pqr.MaterialGroup = vm.Result.MaterialGroup;
                pqr.PNumber = vm.Result.PNumber;

                pqr.ThicknessQualifiedMin = vm.Result.ThicknessQualifiedMin;
                pqr.ThicknessQualifiedMax = vm.Result.ThicknessQualifiedMax;

                pqr.FNumber = vm.Result.FNumber;
                pqr.QualifiedPosition = vm.Result.QualifiedPosition;
                pqr.JointType = vm.Result.JointType;

                pqr.DiameterMin = vm.Result.DiameterMin;
                pqr.DiameterMax = vm.Result.DiameterMax;

                _repo.Update(pqr);
                Load();
            }

        }

        [RelayCommand]
        [SupportedOSPlatform("windows")]
        private void ImportPqr()
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
                var ocrService = new PqrOcrService();
                var parser = new PqrParserService();

                // 1. Convert PDF → Image
                var imagePath = pdfService.ConvertFirstPage(dialog.FileName);

                // 2. Load image
                using var bitmap = new System.Drawing.Bitmap(imagePath);

                // 3. OCR → Text
                var text = ocrService.ExtractTextFromImage(bitmap);

                // 4. Parse → Object
                var parsed = parser.Parse(
                    text,
                    System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));

                // 5. Prevent duplicates
                if (_repo.GetAll().Any(x => x.PqrNumber == parsed.PqrNumber))
                {
                    MessageBox.Show("PQR already exists");
                    return;
                }

                // 6. Save
                _repo.Add(parsed);

                // 7. Refresh UI
                Load();

                MessageBox.Show("PQR imported successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}");
            }
        }

        // =========================
        // DELETE
        // =========================
        [RelayCommand]
        private void DeletePqr()
        {
            if (SelectedPqr == null)
                return;

            var result = MessageBox.Show(
                $"Delete PQR: {SelectedPqr.PqrNumber}?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _repo.Delete(SelectedPqr.Id);
            Load();
        }
    }
}