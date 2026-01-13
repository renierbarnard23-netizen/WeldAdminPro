using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class ProjectDetailsViewModel : ObservableObject
    {
        private readonly ProjectRepository _repo;
        private readonly Project _project;

        public event Action? RequestClose;

        [ObservableProperty]
        private string projectName;

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ProjectDetailsViewModel(Project project)
        {
            _repo = new ProjectRepository();
            _project = project;

            projectName = project.ProjectName;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Save()
        {
            _project.ProjectName = ProjectName;
            _repo.Update(_project);
            RequestClose?.Invoke();
        }
    }
}
