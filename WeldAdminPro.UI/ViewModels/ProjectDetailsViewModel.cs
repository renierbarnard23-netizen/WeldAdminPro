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
        private string projectNumber;

        [ObservableProperty]
        private string projectName;

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ProjectDetailsViewModel(Project project)
        {
            _repo = new ProjectRepository();
            _project = project;

            projectNumber = project.ProjectNumber;
            projectName = project.Title;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Save()
        {
            _project.Title = ProjectName;
            _repo.Update(_project);
            RequestClose?.Invoke();
        }
    }
}
