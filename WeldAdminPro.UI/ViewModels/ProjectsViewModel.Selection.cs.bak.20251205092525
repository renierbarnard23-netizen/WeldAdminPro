using System;
using System.Linq;
using System.Threading.Tasks;
using WeldAdminPro.Core.Models;
using WeldAdminPro.UI.Services;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProjectsViewModel
    {
        private readonly UserStateService _state = new UserStateService();

        private Project? _selectedProject;
        /// <summary>
        /// Selected project in the list. Setting this will persist the selection so it can be restored next run.
        /// </summary>
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    SaveSelectedProjectAsync();
                }
            }
        }

        private async void SaveSelectedProjectAsync()
        {
            try
            {
                if (SelectedProject == null)
                {
                    _state.SetSelectedProjectId(null);
                    return;
                }

                // prefer Id property if present, otherwise fall back to ProjectNumber
                var id = TryGetProjectIdentifier(SelectedProject);
                if (!string.IsNullOrEmpty(id))
                    _state.SetSelectedProjectId(id);
            }
            catch { }
        }

        private static string? TryGetProjectIdentifier(Project p)
        {
            try
            {
                var t = p.GetType();
                var idProp = t.GetProperty("Id") ?? t.GetProperty("IdString") ?? t.GetProperty("ProjectId");
                if (idProp != null)
                {
                    var v = idProp.GetValue(p);
                    if (v != null) return v.ToString();
                }

                var pn = t.GetProperty("ProjectNumber") ?? t.GetProperty("Number");
                if (pn != null)
                {
                    var v = pn.GetValue(p);
                    if (v != null) return v.ToString();
                }

                // last resort - use Title + StartDate as a crude key
                var title = t.GetProperty("Title")?.GetValue(p)?.ToString() ?? "";
                var sd = t.GetProperty("StartDate")?.GetValue(p)?.ToString() ?? "";
                return (title + "|" + sd).Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// After RefreshAsync has repopulated Projects, attempt to restore previously selected project.
        /// </summary>
        private void TryRestoreSelectedProject()
        {
            try
            {
                var savedId = _state.GetSelectedProjectId();
                if (string.IsNullOrEmpty(savedId)) return;

                var match = Projects.FirstOrDefault(p =>
                {
                    var id = TryGetProjectIdentifier(p);
                    return !string.IsNullOrEmpty(id) && id == savedId;
                });

                if (match != null)
                {
                    // ensure selection is set on UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedProject = match;
                    });
                }
            }
            catch { }
        }

        // call TryRestoreSelectedProject at the end of RefreshAsync
        private void OnRefreshCompleted()
        {
            TryRestoreSelectedProject();
        }
    }
}

