using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly ProjectRepository _repo;

        [ObservableProperty]
        private ObservableCollection<Project> projects = new();

        [ObservableProperty]
        private Project? selectedProject;

        public IRelayCommand NewProjectCommand { get; }
        public IRelayCommand OpenProjectCommand { get; }

        public ProjectsViewModel()
        {
            _repo = new ProjectRepository();
            LoadProjects();

            NewProjectCommand = new RelayCommand(OpenNewProject);
            OpenProjectCommand = new RelayCommand(OpenSelectedProject);
        }

        private void LoadProjects()
        {
            Projects = new ObservableCollection<Project>(_repo.GetAll());
        }

        private void OpenNewProject()
        {
            var vm = new NewProjectViewModel();

            var window = new NewProjectWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };

            vm.ProjectCreated += project =>
            {
                _repo.Add(project);
                Projects.Add(project);
            };

            vm.RequestClose += () => window.Close();

            window.ShowDialog();
        }

        private void OpenSelectedProject()
        {
            if (SelectedProject == null)
                return;

            var vm = new ProjectDetailsViewModel(SelectedProject);

            var window = new ProjectDetailsWindow(vm)
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();
            LoadProjects();
        }
    }
}
