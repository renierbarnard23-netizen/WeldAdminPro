using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private object? _currentView;

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowHomeCommand { get; }
        public ICommand ShowProjectsCommand { get; }
        public ICommand ShowReportsCommand { get; }

        public MainViewModel()
        {
            ShowHomeCommand = new RelayCommand(ShowHome);
            ShowProjectsCommand = new RelayCommand(ShowProjects);
            ShowReportsCommand = new RelayCommand(ShowReports);

            ShowHome();
        }

        private void ShowHome()
        {
            CurrentView = new HomeView();
        }

        private void ShowProjects()
        {
            CurrentView = new ProjectsView();
        }

        private void ShowReports()
        {
            CurrentView = new ReportsView();
        }
    }
}
