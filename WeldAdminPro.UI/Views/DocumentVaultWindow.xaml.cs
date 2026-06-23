using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Services;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace WeldAdminPro.UI.Views
{
    public partial class DocumentVaultWindow
        : Window
    {
        private readonly DocumentVaultViewModel
            _viewModel;

        public DocumentVaultWindow()
        {
            InitializeComponent();

            _viewModel =
                new DocumentVaultViewModel();

            DataContext =
                _viewModel;

            ApplySecurity();
        }

        private void DocumentsGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            if (grid.SelectedItem
                is not DocumentVaultFile file)
            {
                return;
            }

            _viewModel.OpenDocumentCommand
                .Execute(file);
        }

        private void UploadDocument_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new AddDocumentWindow();

        var result = 
                window.ShowDialog();

            if (result == true)
            {

                AuditService.Log(
                "UPLOAD DOCUMENT",
                "Document Vault",
                "Document uploaded");

                DataContext =
                    new DocumentVaultViewModel();
            }
            

        }

        private void OpenDocument_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.AccessDocumentVault))
            {
                MessageBox.Show(
                    "You do not have permission to open documents.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (DocumentsGrid.SelectedItem
                is not DocumentVaultFile file)
            {
                MessageBox.Show(
                    "Please select a document.");

                return;
            }

            AuditService.Log(
                "OPEN DOCUMENT",
                "Document Vault",
                $"Opened document: {file.FileName}");

            _viewModel.OpenDocumentCommand
                .Execute(file);
        }

        private void DeleteDocument_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.DeleteDocuments))
            {
                MessageBox.Show(
                    "You do not have permission to delete documents.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (DocumentsGrid.SelectedItem
                is not DocumentVaultFile file)
            {
                MessageBox.Show(
                    "Please select a document.");

                return;
            }

            var result =
                MessageBox.Show(
                    $"Delete '{file.FileName}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // TODO:
            // Add actual repository delete later

            AuditService.Log(
                "DELETE DOCUMENT",
                "Document Vault",
                $"Deleted document: {file.FileName}");

            MessageBox.Show(
                "Delete functionality placeholder completed.");
        }

        private void ApplySecurity()
        {
            UploadButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.UploadDocuments);

            DeleteButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.DeleteDocuments);

            OpenButton.IsEnabled =
                PermissionService.HasPermission(
                    SystemPermission.AccessDocumentVault);
        }
    }
}
