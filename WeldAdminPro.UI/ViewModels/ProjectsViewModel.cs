using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public class ProjectsViewModel
    {
        private readonly ProjectRepository _repo = new();

        public ObservableCollection<Project> Projects { get; } = new();

        public ProjectsViewModel()
        {
            LoadProjects();
        }

        private void LoadProjects()
        {
            Projects.Clear();
            foreach (var p in _repo.GetAll())
                Projects.Add(p);
        }
    }
}
