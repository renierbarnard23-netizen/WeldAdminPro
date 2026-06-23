using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using Microsoft.Win32;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

public partial class QualityViewModel : ObservableObject
{
    private readonly ProjectDocumentRepository _repo = new();
    private readonly ProjectDocumentService _service = new();

    [ObservableProperty]
    private ObservableCollection<ProjectDocument> documents = new();

    [ObservableProperty]
    private string projectStatus = "";

    [ObservableProperty]
    private Brush projectStatusColor = Brushes.Green;

    private Guid _currentProjectId = Guid.NewGuid(); // or set from selected project

    public void Load()
    {
        _service.InitializeProjectDocuments(_currentProjectId);

        var docs = _repo.GetByProject(_currentProjectId);

        var complianceService = new ProjectComplianceService();
        var result = complianceService.Evaluate(_currentProjectId);

        if (result.IsCompliant)
        {
            ProjectStatus = "✔ PROJECT COMPLIANT";
            ProjectStatusColor = Brushes.Green;
        }
        else
        {
            ProjectStatus = "❌ PROJECT NOT COMPLIANT";
            ProjectStatusColor = Brushes.Red;
        }

        Documents = new ObservableCollection<ProjectDocument>(docs);
    }

    [RelayCommand]
    private void Refresh()
    {
        Load();
    }

    // ✅ FIXED: safe command handling
    [RelayCommand]
    private void Upload(object obj)
    {
        if (obj is not ProjectDocument doc)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Select Document",
            Filter = "All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var selectedFile = dialog.FileName;

            var targetFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WeldAdminDocs",
                $"Project_{doc.ProjectId}"
            );

            Directory.CreateDirectory(targetFolder);

            var fileName = Path.GetFileName(selectedFile);
            var destinationPath = Path.Combine(targetFolder, fileName);

            File.Copy(selectedFile, destinationPath, true);

            doc.FilePath = destinationPath;
            doc.IsUploaded = true;
            doc.UploadedDate = DateTime.Now;

            _repo.Update(doc);

            Load();
        }
    }

    // ✅ FIXED: safe command handling
    [RelayCommand]
    private void Open(object obj)
    {
        if (obj is not ProjectDocument doc)
            return;

        if (string.IsNullOrEmpty(doc.FilePath))
            return;

        if (File.Exists(doc.FilePath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = doc.FilePath,
                UseShellExecute = true
            });
        }
    }
}