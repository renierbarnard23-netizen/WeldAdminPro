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
        [ObservableProperty] private bool isLocked;
        public PqrViewModel()
        {
            Load();
        }


        partial void OnSelectedPqrChanged(Pqr? value)
        {
            IsLocked = value?.IsLocked ?? false;
        }

        public void Load()
        {
            try
            {
                PqrList.Clear();

                var data = _repo.GetAll();

                foreach (var p in data)
                    PqrList.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load PQRs: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            Load();
        }
        public ObservableCollection<string> WeldingProcesses { get; } =
    new ObservableCollection<string>
    {
        "SMAW",
        "GMAW",
        "GTAW",
        "FCAW",
        "SAW"
    };

        [ObservableProperty]
        private string selectedWeldingProcess = "TIG";

        // =========================
        // ADD
        // =========================
        [RelayCommand]
        private void AddPqr()
        {


            var vm = new PqrEditorViewModel();

            System.Diagnostics.Debug.WriteLine("Dialog closed");
            System.Diagnostics.Debug.WriteLine(vm.Result?.PqrNumber ?? "NO RESULT");

            var view = new PqrEditorView
            {
                DataContext = vm
            };

            view.ShowDialog();

           

            // ✅ SAVE after closing
            if (vm.Result == null)
            {
                MessageBox.Show("Nothing was saved.", "Info");
                return;
            }

            _repo.Add(vm.Result);
            Load();

            MessageBox.Show("PQR saved successfully.", "Success");
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

            if (SelectedPqr.IsLocked)
            {
                MessageBox.Show("Cannot edit an approved (locked) PQR.");
                return;
            }

            var vm = new PqrEditorViewModel(SelectedPqr);

            var view = new PqrEditorView
            {
                DataContext = vm
            };

            view.ShowDialog();

            if (vm.Result == null)
            {
                MessageBox.Show("No changes saved.", "Info");
                return;
            }

            _repo.Update(vm.Result);
            Load();

            MessageBox.Show("PQR updated successfully.", "Success");
        }

        [RelayCommand]
        private void Approve()
        {
            if (SelectedPqr == null)
                return;

            if (SelectedPqr.IsLocked)
            {
                MessageBox.Show("PQR already approved.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedPqr.PqrNumber))
            {
                MessageBox.Show("PQR Number is required before approval.");
                return;
            }

            SelectedPqr.IsApproved = true;
            SelectedPqr.IsLocked = true;
            SelectedPqr.ApprovedOn = DateTime.Now;
            SelectedPqr.ApprovedBy = "User";

            _repo.Update(SelectedPqr);
            Load();

            MessageBox.Show("PQR approved and locked.");
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