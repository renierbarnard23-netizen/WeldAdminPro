using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Mail;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class DocumentAttachmentViewModel
        : ObservableObject
    {
        private readonly Guid _entityId;

        private readonly string _entityType;

        private readonly DocumentAttachmentRepository
            _repository;

        [ObservableProperty]
        private ObservableCollection<DocumentAttachment>
            attachments = new();

        public DocumentAttachmentViewModel(
            Guid entityId,
            string entityType)
        {
            _entityId = entityId;

            _entityType = entityType;

            _repository =
                new DocumentAttachmentRepository(
                    DatabasePath.GetConnectionString());

            Load();
        }

        public void Load()
        {
            Attachments.Clear();

            var items =
                _repository.GetByEntity(
                    _entityId);

            foreach (var item in items)
            {
                Attachments.Add(item);
            }
        }

        [RelayCommand]
        private void Upload()
        {
            var dialog =
                new OpenFileDialog
                {
                    Multiselect = false
                };

            if (dialog.ShowDialog() != true)
                return;

            var storageFolder =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Attachments");

            Directory.CreateDirectory(
                storageFolder);

            var fileName =
                Path.GetFileName(
                    dialog.FileName);

            var destination =
                Path.Combine(
                    storageFolder,
                    fileName);

            File.Copy(
                dialog.FileName,
                destination,
                true);

            var attachment =
                new DocumentAttachment
                {
                    Id = Guid.NewGuid(),

                    RelatedEntityId =
                        _entityId,

                    EntityType =
                        _entityType,

                    FileName =
                        fileName,

                    FilePath =
                        destination,

                    UploadedBy =
                        Environment.UserName,

                    UploadedDate =
                        DateTime.UtcNow,

                    Category =
                        "General"
                };

            _repository.Add(
                attachment);

            Load();

            MessageBox.Show(
                "Document uploaded.");
        }
    }
}