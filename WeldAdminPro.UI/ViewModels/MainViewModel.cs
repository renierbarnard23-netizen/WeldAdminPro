using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.UI.Views;
using System.Windows;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class MainViewModel
        : ObservableObject
    {
        // =====================================
        // CURRENT VIEW
        // =====================================

        [ObservableProperty]
        private object? currentView;

        // =====================================
        // SELECTED PROJECT
        // =====================================

        [ObservableProperty]
        private Project? selectedProject;

        public ICommand ShowHomeCommand { get; }
        public ICommand ShowProjectsCommand { get; }
        public ICommand ShowReportsCommand { get; }

        public MainViewModel()
        {
            ShowHomeCommand =
                new RelayCommand(ShowHome);

            ShowProjectsCommand =
                new RelayCommand(ShowProjects);

            ShowReportsCommand =
                new RelayCommand(ShowReports);

            OnPropertyChanged(
                nameof(UserManagementVisibility));

            ShowHome();

            OnPropertyChanged(
                nameof(CurrentUserDisplay));
        }

        public Visibility UserManagementVisibility =>
            PermissionService.CanManageUsers
                ? Visibility.Visible
                : Visibility.Collapsed;

        private void ShowHome()
        {
            CurrentView =
                new HomeView();
        }

        private void ShowProjects()
        {
            CurrentView =
                new ProjectsView(
                    new ProjectsViewModel());
        }

        private void ShowReports()
        {
            CurrentView =
                new ReportsView();
        }

        // =====================================
        // CURRENT USER DISPLAY
        // =====================================

        public string CurrentUserDisplay =>
            $"Logged in as: {CurrentUserContext.Username}";

        public CurrentUserPermissionsViewModel
            Permissions
        { get; private set; }
                = new();
    }
}
