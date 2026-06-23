using System;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality;
using System.Diagnostics;
using System.Windows.Input;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class DocumentAttachmentPanel
        : UserControl
    {
        public DocumentAttachmentPanel(
            Guid entityId,
            string entityType)
        {
            InitializeComponent();

            DataContext =
                new DocumentAttachmentViewModel(
                    entityId,
                    entityType);
        }

        private void DataGrid_MouseDoubleClick(
    object sender,
    MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid)
                return;

            if (grid.SelectedItem
                is not DocumentAttachment attachment)
            {
                return;
            }

            if (!System.IO.File.Exists(
                    attachment.FilePath))
            {
                return;
            }

            Process.Start(
                new ProcessStartInfo
                {
                    FileName =
                        attachment.FilePath,

                    UseShellExecute = true
                });
        }
    }


}