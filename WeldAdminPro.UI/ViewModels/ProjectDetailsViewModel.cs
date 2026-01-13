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

        [ObservableProperty]
        private DateTime? startDate;

        [ObservableProperty]
        private DateTime? endDate;

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ProjectDetailsViewModel(Project project)
        {
            _repo = new ProjectRepository();
            _project = project;

            ProjectNumber = project.ProjectNumber;
            ProjectName = project.ProjectName;
            StartDate = project.StartDate;
            EndDate = project.EndDate;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void Save()
        {
            _project.ProjectName = ProjectName;
            _project.StartDate = StartDate;
            _project.EndDate = EndDate;

            _repo.Update(_project);
            RequestClose?.Invoke();
        }
    }
}
