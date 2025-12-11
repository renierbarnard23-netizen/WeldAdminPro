using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace WeldAdminPro.UI.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public object? CurrentView { get; set; }

        public ICommand ShowProjectsCommand { get; }
        public ICommand ShowDatabookCommand { get; }
        public ICommand ShowQcpCommand { get; }
        public ICommand ShowInspectionsCommand { get; }
        public ICommand ShowReportsCommand { get; }

        public MainViewModel()
        {
            ShowProjectsCommand = new RelayCommand(() => CurrentView = "Projects view placeholder");
            ShowDatabookCommand = new RelayCommand(() => CurrentView = "Databook view placeholder");
            ShowQcpCommand = new RelayCommand(() => CurrentView = "QCP view placeholder");
            ShowInspectionsCommand = new RelayCommand(() => CurrentView = "Inspections view placeholder");
            ShowReportsCommand = new RelayCommand(() => CurrentView = "Reports view placeholder");
        }
    }
}
