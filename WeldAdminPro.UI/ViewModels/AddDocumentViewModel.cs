using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.Core.Reporting.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class AddDocumentViewModel
        : ObservableObject
    {
        private readonly DocumentVaultService
            _vaultService = new();

        private readonly DocumentVaultRepository 
            _repository = new(DatabasePath.GetConnectionString());

        [ObservableProperty]
        private string filePath = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private string revision = "0";

        [ObservableProperty]
        private string uploadedBy =
            Environment.UserName;

        [ObservableProperty]
        private bool isApproved = true;

        [ObservableProperty]
        private DocumentCategoryType selectedCategory =
            DocumentCategoryType.WPS;

        [ObservableProperty]
        private string documentNumber = "";

        [ObservableProperty]
        private string title = "";


        public ObservableCollection<DocumentCategoryType>
            Categories
        { get; }
            = new(
                Enum.GetValues<DocumentCategoryType>());

        public Action<DocumentVaultFile>? OnSaved;

        [RelayCommand]
        private void Browse()
        {
            var dialog =
                new OpenFileDialog
                {
                    Filter =
                        "PDF Files (*.pdf)|*.pdf"
                };

            if (dialog.ShowDialog() == true)
            {
                FilePath =
                    dialog.FileName;
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                return;
            }

            var savedPath =
                _vaultService.SaveDocument(
                    FilePath,
                    SelectedCategory.ToString());

            if (string.IsNullOrWhiteSpace(savedPath))
            {
                return;
            }

            var file =
                new DocumentVaultFile
                {
                    Id = Guid.NewGuid(),

                    FileName =
                        Path.GetFileName(savedPath),

                    OriginalFileName =
                        Path.GetFileName(FilePath),

                    FilePath =
                        savedPath,

                    Category =
                        SelectedCategory,

                    Description =
                        Description,

                    DocumentNumber =
                        DocumentNumber,

                    Title = 
                        Title,

                    Status =
                        IsApproved
                            ? "Approved"
                            : "Draft",

                    Revision =
                        Revision,

                    UploadedDate =
                        DateTime.Now,

                    UploadedBy =
                        UploadedBy,

                    IsApproved =
                        IsApproved
                };

                _repository.Add(file);

                OnSaved?.Invoke(file);

                MessageBox.Show(
                    "Document uploaded successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

        }
    }
}
