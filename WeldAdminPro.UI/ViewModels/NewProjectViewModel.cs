using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class NewProjectViewModel : ObservableObject
    {
        [ObservableProperty]
        private Project project = new();

        public event Action<Project>? ProjectCreated;
        public event Action? RequestClose;

        public NewProjectViewModel()
        {
            // Auto-generate project number when window opens
            Project.ProjectNumber = $"PRJ-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        [RelayCommand] // ✅ ONLY ONCE
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Project.ProjectName))
                return; // optional validation

            Project.Id = Guid.NewGuid();

            ProjectCreated?.Invoke(Project);
            RequestClose?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
}
