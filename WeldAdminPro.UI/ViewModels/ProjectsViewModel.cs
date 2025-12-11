using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly WeldAdminDbContext _db;
        private readonly Services.ExcelExportService _excel;
        private const string LastProjectFile = "lastproject.json";

        public ObservableCollection<Project> Projects { get; } = new ObservableCollection<Project>();

        private string _filter = string.Empty;
        public string Filter
        {
            get => _filter;
            set
            {
                if (SetProperty(ref _filter, value))
                {
                    _ = RefreshAsync();
                }
            }
        }

        private Project? _selectedProject;
        /// <summary>
        /// Bound to DataGrid.SelectedItem. When changed we persist the selection to disk (ProjectNumber).
        /// </summary>
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    // persist selection (fire-and-forget)
                    _ = SaveSelectedProjectAsync();
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand ExportExcelCommand { get; }

        public ProjectsViewModel()
        {
            var options = new DbContextOptionsBuilder<WeldAdminDbContext>()
                .UseSqlite("Data Source=weldadmin.db")
                .Options;

            _db = new WeldAdminDbContext(options);
            _excel = new Services.ExcelExportService();

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            ExportExcelCommand = new AsyncRelayCommand(ExportExcelAsync);

            // initial load (fire-and-forget)
            _ = RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            try
            {
                var q = _db.Projects.AsQueryable();

                if (!string.IsNullOrWhiteSpace(Filter))
                {
                    var f = Filter.Trim();
                    q = q.Where(p => p.ProjectNumber.Contains(f) || p.Title.Contains(f) || p.Client.Contains(f));
                }

                var list = await q.OrderBy(p => p.ProjectNumber).ToListAsync();

                // update collection on UI thread
                App.Current.Dispatcher.Invoke(() =>
                {
                    Projects.Clear();
                    foreach (var p in list) Projects.Add(p);
                });

                // After loading, attempt to restore previous selection (if any)
                await RestoreSelectedProjectAsync(list);
            }
            catch (Exception ex)
            {
                // swallow or log as appropriate; avoid throwing during data load
                System.Diagnostics.Debug.WriteLine($"ProjectsViewModel.RefreshAsync failed: {ex}");
            }
        }

        private async Task ExportExcelAsync()
        {
            var rows = Projects.Select(p => new
            {
                p.ProjectNumber,
                p.Title,
                p.Client,
                StartDate = p.StartDate?.ToString("yyyy-MM-dd"),
                p.DatabookVersion
            }).ToList();

            await _excel.ExportProjectsAsync(rows, "ProjectsExport.xlsx");
        }

        private string GetLastProjectPath()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(baseDir, LastProjectFile);
            }
            catch
            {
                return LastProjectFile;
            }
        }

        private async Task SaveSelectedProjectAsync()
        {
            try
            {
                var path = GetLastProjectPath();
                var model = SelectedProject is null ? null : new { ProjectNumber = SelectedProject.ProjectNumber };
                var json = JsonSerializer.Serialize(model);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveSelectedProjectAsync failed: {ex}");
            }
        }

        private async Task RestoreSelectedProjectAsync(System.Collections.Generic.List<Project> currentList)
        {
            try
            {
                var path = GetLastProjectPath();
                if (!File.Exists(path)) return;

                var json = await File.ReadAllTextAsync(path);
                if (string.IsNullOrWhiteSpace(json)) return;

                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                if (doc.ValueKind == JsonValueKind.Null) return;
                if (doc.TryGetProperty("ProjectNumber", out var pn))
                {
                    var projectNumber = pn.GetString();
                    if (!string.IsNullOrWhiteSpace(projectNumber))
                    {
                        // find in the freshly loaded list
                        var match = currentList.FirstOrDefault(p => p.ProjectNumber == projectNumber);
                        if (match != null)
                        {
                            // set on UI thread to ensure binding updates
                            App.Current.Dispatcher.Invoke(() => SelectedProject = match);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RestoreSelectedProjectAsync failed: {ex}");
            }
        }
    }
}
