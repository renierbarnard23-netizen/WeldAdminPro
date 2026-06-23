using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using System.IO;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class DocumentVaultViewModel
        : ObservableObject
    {
        private readonly DocumentVaultRepository
            _repository =
                new(DatabasePath.GetConnectionString());

        public ObservableCollection<DocumentVaultFile>
            Documents
        { get; }
            = new();

        public DocumentVaultViewModel()
        {
            LoadDocuments();
        }

        [RelayCommand]
        private void OpenDocument(
        DocumentVaultFile? file)
        {
            if (file == null)
            {
                return;
            }

            if (!File.Exists(file.FilePath))
            {
                MessageBox.Show(
                    "File not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                });
        }


        private void LoadDocuments()
        {
            Documents.Clear();

            var files =
                _repository.GetAll();

            foreach (var file in files
                .OrderByDescending(x => x.UploadedDate))
            {
                Documents.Add(file);
            }
        }
    }
}
